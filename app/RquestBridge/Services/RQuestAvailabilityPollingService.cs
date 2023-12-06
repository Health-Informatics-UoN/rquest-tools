using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RquestBridge.Config;
using RquestBridge.Constants;
using RquestBridge.Dto;

namespace RquestBridge.Services;

public class RQuestAvailabilityPollingService
{
  private readonly BridgeOptions _bridgeOptions;
  private readonly CrateGenerationService _crateGenerationService;
  private readonly HutchApiClient _hutchApiClient;
  private readonly RabbitJobQueueService _jobQueue;
  private readonly ILogger<RQuestAvailabilityPollingService> _logger;
  private readonly MinioService _minioService;
  private readonly RQuestTaskApiClient _taskApi;


  public RQuestAvailabilityPollingService(
    RQuestTaskApiClient taskApi,
    ILogger<RQuestAvailabilityPollingService> logger,
    RabbitJobQueueService jobQueue,
    CrateGenerationService crateGenerationService,
    HutchApiClient hutchApiClient,
    MinioService minioService,
    IOptions<BridgeOptions> bridgeOptions)
  {
    _logger = logger;
    _taskApi = taskApi;
    _jobQueue = jobQueue;
    _crateGenerationService = crateGenerationService;
    _hutchApiClient = hutchApiClient;
    _minioService = minioService;
    _bridgeOptions = bridgeOptions.Value;
  }

  public async Task Poll(RQuestOptions rQuest)
  {
    AvailabilityQuery? job = null;

    do
    {
      try
      {
        // Fetch RQuest Query
        job = await _taskApi.FetchQuery<AvailabilityQuery>(rQuest);
        if (job is null)
        {
          _logger.LogInformation(
            "No Queries on Collection: {CollectionId}",
            rQuest.CollectionId);
          return;
        }

        // Build RQuest RO-Crate
        var bagItPath = Path.Combine(_bridgeOptions.WorkingDirectoryBase, job.Uuid);
        var archive = await _crateGenerationService.BuildBagIt(bagItPath);
        await _crateGenerationService.BuildCrate(job, archive);

        // Zip the BagIt package
        if (!Directory.Exists(bagItPath))
          Directory.CreateDirectory(bagItPath);

        ZipFile.CreateFromDirectory(bagItPath, bagItPath + ".zip");

        // Write generated RO-Crate to store
        await _minioService.WriteToStore(Path.Combine(_bridgeOptions.WorkingDirectoryBase, job.Uuid) + ".zip");
        // Submit RQuest Workflow RO-Crate to HutchAgent
        await _hutchApiClient.HutchEndpointPost(job.Uuid);
      }
      catch (Exception e)
      {
        if (job is null)
        {
          _logger.LogError(e, "Task fetching failed");
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

  private void SendToQueue(RQuestJob jobPayload, string queueName)
  {
    _jobQueue.SendMessage(queueName, jobPayload);
    _logger.LogInformation("Sent to Queue {Body}", JsonSerializer.Serialize(jobPayload));
  }
}
