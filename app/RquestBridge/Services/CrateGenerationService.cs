using System.Text.Json;
using System.Text.RegularExpressions;
using FiveSafes.Net;
using FiveSafes.Net.Constants;
using FiveSafes.Net.Utilities;
using Flurl;
using Microsoft.Extensions.Options;
using ROCrates;
using RquestBridge.Config;
using RquestBridge.Constants;
using RquestBridge.Dto;
using RquestBridge.Utilities;
using File = System.IO.File;

namespace RquestBridge.Services;

public class CrateGenerationService(ILogger<CrateGenerationService> logger,
  IOptions<CratePublishingOptions> publishingOptions,
  IOptions<CrateAgentOptions> agentOptions,
  IOptions<CrateProjectOptions> projectOptions,
  IOptions<CrateOrganizationOptions> organizationOptions,
  IOptions<WorkflowOptions> workflowOptions,
  IOptions<AssessActionsOptions> assessActions)
{
  private readonly CrateAgentOptions _crateAgentOptions = agentOptions.Value;
  private readonly CrateOrganizationOptions _crateOrganizationOptions = organizationOptions.Value;
  private readonly CrateProjectOptions _crateProjectOptions = projectOptions.Value;
  private readonly CratePublishingOptions _publishingOptions = publishingOptions.Value;
  private readonly WorkflowOptions _workflowOptions = workflowOptions.Value;
  private readonly AssessActionsOptions _assessActionsOptions = assessActions.Value;

  /// <summary>
  /// Build an RO-Crate for a cohort discovery query.
  /// </summary>
  /// <typeparam name="T">The type of the query. Choices: <see cref="AvailabilityQuery"/>, <see cref="DistributionQuery"/></typeparam>
  /// <param name="job">The job to save to the crate.</param>
  /// <param name="bagItPath">The BagItArchive path to save the crate to.</param>
  /// <returns></returns>
  /// <exception cref="NotImplementedException">Query type is unavailable.</exception>
  public async Task<BagItArchive> BuildCrate<T>(T job, string bagItPath) where T : class, new()
  {
    var isAvailability = new T() switch
    {
      AvailabilityQuery => true,
      DistributionQuery => false,
      _ => throw new NotImplementedException()
    };


    var workflowUri = GetWorkflowUrl();
    var archive = await BuildBagIt(bagItPath, workflowUri);
    var payload = JsonSerializer.Serialize<T>(job);
    var payloadDestination = Path.Combine(archive.PayloadDirectoryPath, RquestQuery.FileName);
    await SaveJobPayload(payload, payloadDestination);
    logger.LogInformation($"Saved query JSON to {payloadDestination}.");

    // Generate ROCrate metadata
    logger.LogInformation("Building Five Safes ROCrate...");
    var builder = new RQuestWorkflowCrateBuilder(_workflowOptions, _publishingOptions, _crateAgentOptions,
      _crateProjectOptions, _crateOrganizationOptions, archive.PayloadDirectoryPath);
    ROCrate crate = BuildFiveSafesCrate(builder, RquestQuery.FileName, isAvailability);
    crate.Save(archive.PayloadDirectoryPath);
    logger.LogInformation($"Saved Five Safes ROCrate to {archive.PayloadDirectoryPath}");
    await archive.WriteManifestSha512();
    await archive.WriteTagManifestSha512();

    return archive;
  }

  /// <summary>
  /// Save a job payload to a file on disk.
  /// </summary>
  /// <param name="payload">The string to write to the file.</param>
  /// <param name="destination">The path to the file to be created.</param>
  /// <returns></returns>
  private async Task SaveJobPayload(string payload, string destination)
  {
    var destinationInfo = new FileInfo(destination);

    using var fileStream = destinationInfo.Create();
    using var writer = new StreamWriter(fileStream);
    await writer.WriteAsync(payload);
  }

  /// <summary>
  /// Build BagIt archive
  /// </summary>
  /// <param name="destination"></param>
  /// <param name="workflowUri"></param>
  /// <returns></returns>
  public async Task<BagItArchive> BuildBagIt(string destination, string workflowUri)
  {
    var builder = new FiveSafesBagItBuilder(destination);
    builder.BuildCrate(workflowUri);
    await builder.BuildChecksums();
    await builder.BuildTagFiles();
    return builder.GetArchive();
  }

  public ROCrate BuildFiveSafesCrate(RQuestWorkflowCrateBuilder builder, string queryFileName, bool isAvailability)
  {
    builder.AddLicense();
    builder.AddCreateAction(queryFileName, isAvailability);
    builder.AddAgent();
    builder.UpdateMainEntity();
    ROCrate crate = builder.GetROCrate();
    return crate;
  }

  /// <summary>
  /// Construct the Workflow URL from WorkflowOptions.
  /// </summary>
  /// <returns>Workflow URL</returns>
  public string GetWorkflowUrl()
  {
    return Url.Combine(_workflowOptions.BaseUrl, _workflowOptions.Id.ToString())
      .SetQueryParam("version", _workflowOptions.Version.ToString());
  }

  public async Task AssessBagIt(BagItArchive archive)
  {
    var builder = new RQuestWorkflowCrateBuilder(_workflowOptions, _publishingOptions, _crateAgentOptions,
      _crateProjectOptions, _crateOrganizationOptions, archive.PayloadDirectoryPath);
    if (_assessActionsOptions.CheckValue)
    {
      var manifestPath = Path.Combine(archive.ArchiveRootPath, BagItConstants.ManifestPath);
      var tagManifestPath = Path.Combine(archive.ArchiveRootPath, BagItConstants.TagManifestPath);

      var bothFilesExist = File.Exists(manifestPath) && File.Exists(tagManifestPath);
      var checkSumsMatch = await ChecksumsMatch(manifestPath, archive.ArchiveRootPath) &&
                           await ChecksumsMatch(tagManifestPath, archive.ArchiveRootPath);

      if (bothFilesExist && checkSumsMatch)
      {
        builder.AddCheckValueAssessAction(ActionStatus.CompletedActionStatus, DateTime.Now);
      }
      else
      {
        builder.AddCheckValueAssessAction(ActionStatus.FailedActionStatus, DateTime.Now);
      }
    }

    if (_assessActionsOptions.Validate)
    {
      builder.AddValidateCheck(ActionStatus.CompletedActionStatus);
    }

    if (_assessActionsOptions.SignOff)
    {
      builder.AddSignOff();
    }

    var crate = builder.GetROCrate();
    crate.Save(archive.PayloadDirectoryPath);
    await archive.WriteTagManifestSha512();
    await archive.WriteManifestSha512();
  }

  /// <summary>
  /// Check that the actual checksums of the files match the recorded checksums.
  /// </summary>
  /// <param name="checksumFilePath">The path to the checksum file containing records that need validating.</param>
  /// <param name="archiveRoot">Path to the root of the archive.</param>
  /// <returns></returns>
  private async Task<bool> ChecksumsMatch(string checksumFilePath, string archiveRoot)
  {
    var lines = await File.ReadAllLinesAsync(checksumFilePath);
    foreach (var line in lines)
    {
      var checksumAndFile = Regex.Split(line, @"\s+");
      var expectedChecksum = checksumAndFile.First();
      var fileName = checksumAndFile.Last();

      using var fileStream = File.OpenRead(Path.Combine(archiveRoot, fileName));
      var fileChecksum = ChecksumUtility.ComputeSha512(fileStream);
      if (fileChecksum != expectedChecksum) return false;
    }

    return true;
  }
}
