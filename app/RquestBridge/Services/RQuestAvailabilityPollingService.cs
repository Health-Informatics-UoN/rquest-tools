using System.Text.Json;
using Microsoft.Extensions.Logging;
using RquestBridge.Constants;
using RquestBridge.Dto;
using RquestBridge.Config;

namespace RquestBridge.Services;

public class RQuestAvailabilityPollingService
{
  private readonly RQuestTaskApiClient _taskApi;
  private readonly ILogger<RQuestAvailabilityPollingService> _logger;
  private readonly RabbitJobQueueService _jobQueue;
  private readonly CrateGenerationService _crateGenerationService;

  public RQuestAvailabilityPollingService(
    RQuestTaskApiClient taskApi,
    ILogger<RQuestAvailabilityPollingService> logger,
    RabbitJobQueueService jobQueue,
    CrateGenerationService crateGenerationService)
  {
    _logger = logger;
    _taskApi = taskApi;
    _jobQueue = jobQueue;
    _crateGenerationService = crateGenerationService;
  }

  public async Task Poll(RQuestOptions rQuest)
  {
    AvailabilityQuery? job = null;

    do
    {
      try
      {
        job = await _taskApi.FetchQuery<AvailabilityQuery>(rQuest);
        if (job is null)
        {
          _logger.LogInformation(
            "No Queries on Collection: {CollectionId}",
            rQuest.CollectionId);
          return;
        }

        // var packagedJob = PackageJob(job, rQuest);
        var payload = JsonSerializer.SerializeToElement(job);
        
        
        //SendToQueue(packagedJob, "jobs");
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
