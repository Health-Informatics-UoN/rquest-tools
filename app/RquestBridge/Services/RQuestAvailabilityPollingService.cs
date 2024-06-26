using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using RquestBridge.Config;
using RquestBridge.Constants;
using RquestBridge.Dto;

namespace RquestBridge.Services;

public class RQuestAvailabilityPollingService(RQuestTaskApiClient taskApi,
  ILogger<RQuestAvailabilityPollingService> logger,
  CrateGenerationService crateGenerationService,
  HutchApiClient hutchApiClient,
  MinioService minioService,
  IOptions<BridgeOptions> bridgeOptions,
  IFeatureManager features)
{
  private readonly BridgeOptions _bridgeOptions = bridgeOptions.Value;


  public async Task Poll(RQuestOptions rQuest)
  {
    AvailabilityQuery? job = null;

    do
    {
      try
      {
        // Fetch RQuest Query
        job = await taskApi.FetchQuery<AvailabilityQuery>(rQuest);
        if (job is null)
        {
          logger.LogInformation(
            "No Queries on Collection: {CollectionId}",
            rQuest.CollectionId);
          return;
        }

        // Build RQuest RO-Crate
        var bagItPath = Path.Combine(_bridgeOptions.WorkingDirectoryBase, job.Uuid);
        var archive = await crateGenerationService.BuildCrate(job, bagItPath);

        // Assess RO-Crate
        if (await features.IsEnabledAsync(FeatureFlags.MakeAssessActions))
          await crateGenerationService.AssessBagIt(archive);

        // Zip the BagIt package
        if (!Directory.Exists(bagItPath))
          Directory.CreateDirectory(bagItPath);

        ZipFile.CreateFromDirectory(bagItPath, bagItPath + ".zip");

        // Write generated RO-Crate to store
        await minioService.WriteToStore(Path.Combine(_bridgeOptions.WorkingDirectoryBase, job.Uuid) + ".zip");
        // Submit RQuest Workflow RO-Crate to HutchAgent
        await hutchApiClient.HutchEndpointPost(job.Uuid);
      }
      catch (Exception e)
      {
        if (job is null)
        {
          logger.LogError(e, "Task fetching failed");
        }
        else
        {
          throw;
        }
      }
    } while (job is null);
  }

  private RQuestJob PackageJob(AvailabilityQuery jobPayload, RQuestOptions rQuest)
  {
    var job = new RQuestJob()
    {
      Payload = JsonSerializer.SerializeToElement(jobPayload),
      Type = RQuestJobTypes.AvailabilityQuery,
      JobId = jobPayload.Uuid
    };
    return job;
  }
}
