using System.Net;
using Hutch.Rackit;
using Hutch.Rackit.TaskApi.Contracts;
using Hutch.Rackit.TaskApi.Models;
using Hutch.Relay.Config;
using Hutch.Relay.Models;
using Hutch.Relay.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Hutch.Relay.Services;

public class TaskApiService(
  ILogger<TaskApiService> logger,
  IOptions<ApiClientOptions> options,
  ITaskApiClient upstreamTasks,
  IObfuscationService obfuscation,
  IOptions<ObfuscationOptions> obfuscationOptions)
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
        int count = obfuscation.LowNumberSuppression(jobResult.Results.Count,
          obfuscationOptions.Value.LowNumberSuppressionThreshold);
        count = obfuscation.Rounding(count,
         obfuscationOptions.Value.RoundingTarget);
       
        jobResult.Results.Count = count;
        // Submit results upstream
        await upstreamTasks.SubmitResultAsync(relayTask.Id, jobResult, options.Value);
        logger.LogInformation("Successfully submitted results for {RelayTaskId}", relayTask.Id);
        break;
      }
      catch (RackitApiClientException exception)
      {
        if (exception.UpstreamApiResponse is { StatusCode: HttpStatusCode.InternalServerError })
        {
          retryCount++;
          logger.LogError(
            "Task submission failed with 500 Internal Server Error. Retrying in {delayInSeconds} seconds... ({retryCount}/{maxRetries})",
            delayInSeconds,
            retryCount, maxRetryCount);
          
          await Task.Delay(delayInSeconds * 1000);
        }
      }
    }
  }
}
