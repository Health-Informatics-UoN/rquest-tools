using System.Text.Json;
using FiveSafes.Net;
using Microsoft.Extensions.Options;
using ROCrates;
using RquestBridge.Config;
using RquestBridge.Constants;
using RquestBridge.Dto;
using RquestBridge.Utilities;

namespace RquestBridge.Services;

public class CrateGenerationService
{
  private readonly CrateAgentOptions _crateAgentOptions;
  private readonly CrateOrganizationOptions _crateOrganizationOptions;
  private readonly CrateProfileOptions _crateProfileOptions;
  private readonly CrateProjectOptions _crateProjectOptions;
  private readonly ILogger<CrateGenerationService> _logger;
  private readonly CratePublishingOptions _publishingOptions;
  private readonly WorkflowOptions _workflowOptions;

  public CrateGenerationService(
    ILogger<CrateGenerationService> logger,
    IOptions<CratePublishingOptions> publishingOptions,
    IOptions<CrateAgentOptions> agentOptions,
    IOptions<CrateProjectOptions> projectOptions,
    IOptions<CrateOrganizationOptions> organizationOptions,
    IOptions<WorkflowOptions> workflowOptions, IOptions<CrateProfileOptions> crateProfileOptions
  )
  {
    _logger = logger;
    _crateProfileOptions = crateProfileOptions.Value;
    _publishingOptions = publishingOptions.Value;
    _crateAgentOptions = agentOptions.Value;
    _crateOrganizationOptions = organizationOptions.Value;
    _crateProjectOptions = projectOptions.Value;
    _workflowOptions = workflowOptions.Value;
  }

  /// <summary>
  /// Build an RO-Crate for a cohort discovery query.
  /// </summary>
  /// <typeparam name="T">The type of the query. Choices: <see cref="AvailabilityQuery"/>, <see cref="DistributionQuery"/></typeparam>
  /// <param name="job">The job to save to the crate.</param>
  /// <param name="archive">The BagItArchive to save the crate to.</param>
  /// <returns></returns>
  /// <exception cref="NotImplementedException">Query type is unavailable.</exception>
  public async Task BuildCrate<T>(T job, BagItArchive archive) where T : class, new()
  {
    var isAvailability = new T() switch
    {
      AvailabilityQuery => true,
      DistributionQuery => false,
      _ => throw new NotImplementedException()
    };

    var payload = JsonSerializer.Serialize<T>(job);
    var payloadDestination = Path.Combine(archive.PayloadDirectoryPath, RquestQuery.FileName);
    await SaveJobPayload(payload, payloadDestination);
    _logger.LogInformation($"Saved query JSON to {payloadDestination}.");

    // Generate ROCrate metadata
    _logger.LogInformation("Building Five Safes ROCrate...");
    var builder = new RQuestWorkflowCrateBuilder(_workflowOptions, _publishingOptions, _crateAgentOptions,
      _crateProjectOptions, _crateOrganizationOptions, _crateProfileOptions);
    var director = new RQuestWorkflowCrateDirector(builder);
    director.BuildRQuestWorkflowCrate(RquestQuery.FileName, isAvailability);
    ROCrate crate = builder.GetROCrate();
    crate.Save(archive.PayloadDirectoryPath);
    _logger.LogInformation($"Saved Five Safes ROCrate to {archive.PayloadDirectoryPath}");
    await archive.WriteManifestSha512();
    await archive.WriteTagManifestSha512();
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
  /// 
  /// </summary>
  /// <param name="destination"></param>
  /// <returns></returns>
  public async Task<BagItArchive> BuildBagIt(string destination)
  {
    var builder = new FiveSafesBagItBuilder(destination);
    var packer = new Packer(builder);
    await packer.BuildBlankArchive();
    return builder.GetArchive();
  }
}
