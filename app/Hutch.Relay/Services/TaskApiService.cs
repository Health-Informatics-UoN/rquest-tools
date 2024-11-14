using System.Net;
using Hutch.Rackit;
using Hutch.Rackit.TaskApi.Contracts;
using Hutch.Rackit.TaskApi.Models;
using Hutch.Relay.Models;
using Microsoft.Extensions.Options;

namespace Hutch.Relay.Services;

public class TaskApiService(
  ILogger<TaskApiService> logger,
  IOptions<ApiClientOptions> options,
  ITaskApiClient upstreamTasks)
{
  public async Task SubmitResults(RelayTaskModel relayTask, JobResult jobResult
  )
  {
    int retryCount = 0;
    int delayInSeconds = 5;
    int maxRetryCount = 5;
    while (retryCount < maxRetryCount)
    {
      logger.LogInformation("Submitting Results..");
      try
      {
        // Submit results upstream
        var response = upstreamTasks.SubmitResultAsync(relayTask.Id, jobResult, options.Value);
        if (response.IsCompletedSuccessfully)
        {
          logger.LogError("Successfully submitted results for {RelayTaskId}", relayTask.Id);
        }
      }
      catch (RackitApiClientException exception)
      {
        if (exception.UpstreamApiResponse is { StatusCode: HttpStatusCode.InternalServerError })
        {
          retryCount++;
          logger.LogInformation(
            "Task submission failed with 500 Internal Server Error. Retrying in {delayInSeconds} seconds... ({retryCount}/{maxRetries})",
            delayInSeconds,
            retryCount, maxRetryCount);
          // delay retry
          await Task.Delay(delayInSeconds * 1000);
        }
      }
    }

    logger.LogError("Max retry attempts reached. Returning results for {RelayTaskId} has failed or timed out."
      , relayTask.Id);
  }
}
