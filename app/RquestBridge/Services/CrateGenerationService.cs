using System.Text.Json;
using FiveSafes.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ROCrates;
using ROCrates.Exceptions;
using RquestBridge.Config;
using RquestBridge.Dto;
using RquestBridge.Utilities;

namespace RquestBridge.Services;

public class CrateGenerationService
{
  private readonly CrateAgentOptions _crateAgentOptions;
  private readonly CrateOrganizationOptions _crateOrganizationOptions;
  private readonly CrateProfileOptions _crateProfileOptions;
  private readonly CrateProjectOptions _crateProjectOptions;
  private readonly CratePublishingOptions _publishingOptions;
  private readonly WorkflowOptions _workflowOptions;
  private ILogger<CrateGenerationService> _logger;

  public CrateGenerationService(
    ILogger<CrateGenerationService> logger,
    IOptions<CratePublishingOptions> publishingOptions,
    IOptions<CrateAgentOptions> agentOptions,
    IOptions<CrateProjectOptions> projectOptions,
    IOptions<CrateOrganizationOptions> organizationOptions,
    IOptions<WorkflowOptions> workflowOptions, IOptions<CrateProfileOptions> crateProfileOptions)
  {
    _logger = logger;
    _crateProfileOptions = crateProfileOptions.Value;
    _publishingOptions = publishingOptions.Value;
    _crateAgentOptions = agentOptions.Value;
    _crateOrganizationOptions = organizationOptions.Value;
    _crateProjectOptions = projectOptions.Value;
    _workflowOptions = workflowOptions.Value;
  }

  public async Task BuildCrate<T>(T job) where T : class, new()
  {
    var isAvailability = new T() switch
    {
      AvailabilityQuery => true,
      DistributionQuery => false,
      _ => throw new NotImplementedException()
    };

    var archive = await BuildBagIt(BridgeOptions.WorkingDirectory);

    var payload = JsonSerializer.Serialize<T>(job);
    var payloadDestination = Path.Combine(archive.PayloadDirectoryPath, RquestQueryOptions.FileName);
    await SaveJobPayload(payload, payloadDestination);

    // Generate ROCrate metadata
    var builder = new RQuestWorkflowCrateBuilder(_workflowOptions, _publishingOptions, _crateAgentOptions,
      _crateProjectOptions, _crateOrganizationOptions, _crateProfileOptions);
    var director = new RQuestWorkflowCrateDirector(builder);
    director.BuildRQuestWorkflowCrate(RquestQueryOptions.FileName, isAvailability);
    ROCrate crate = builder.GetROCrate();
    crate.Save(archive.PayloadDirectoryPath);
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
  private async Task<BagItArchive> BuildBagIt(string destination)
  {
    var builder = new FiveSafesBagItBuilder(destination);
    var packer = new Packer(builder);
    await packer.BuildBlankArchive();
    return builder.GetArchive();
  }

  private ROCrate InitialiseCrate(string cratePath)
  {
    var crate = new ROCrate();
    try
    {
      crate.Initialise(cratePath);
    }
    catch (CrateReadException e)
    {
      _logger.LogError(exception: e, "RO-Crate cannot be read, or is invalid");
      throw;
    }
    catch (MetadataException e)
    {
      _logger.LogError(exception: e, "RO-Crate Metadata cannot be read, or is invalid");
      throw;
    }

    return crate;
  }
}
