using Hutch.Rackit;
using Hutch.Rackit.TaskApi;
using Hutch.Rackit.TaskApi.Models;
using Microsoft.Extensions.Options;

namespace Hutch.Relay.Services;

/// <summary>
/// This is a background worker (IHostedService) for polling for tasks from an upstream system (e.g. Relay or BC|RQuest)
/// </summary>
public class UpstreamTaskPoller(
  ILogger<UpstreamTaskPoller> logger,
  IOptions<ApiClientOptions> options,
  TaskApiClient upstreamTasks,
  SubNodeService subNodes,
  RelayTaskService relayTasks,
  RelaySubTaskService relaySubTasks) : BackgroundService
{
  protected override Task ExecuteAsync(CancellationToken stoppingToken)
  {
    // We need to simultaneously poll against all supported task queues in an upstream system
    // TODO: this may become configurable to support upstream unified queues e.g. in Relay

    // Start polling for job types:
    var availabilityQueries = upstreamTasks.PollJobQueue<AvailabilityJob>(options.Value, stoppingToken);
    var collectionAnalyses = upstreamTasks.PollJobQueue<CollectionAnalysisJob>(options.Value, stoppingToken);

    // TODO: var cohortAnalyses = upstreamTasks.PollJobQueue<>(options.Value, stoppingToken);

    // start parallel handler threads
    return Task.WhenAll(
      HandleTasks(availabilityQueries, stoppingToken),
      HandleTasks(collectionAnalyses, stoppingToken));

    // TODO: Currently this will not restart polling in the event of exceptions
    // could probably use `Task.WhenAny` to cancel and restart in a loop after logging?
  }

  private async Task HandleTasks<T>(IAsyncEnumerable<T> jobs, CancellationToken cancellationToken)
    where T : TaskApiBaseResponse
  {
    await foreach (var job in jobs.WithCancellation(cancellationToken))
    {
      logger.LogInformation("Task handled: ({Type}) {Id}", typeof(T).Name, job.Uuid);
      
      var subnodes = await subNodes.List();
      if (subnodes.Count == 0) return;

      // Create a parent task
      await relayTasks.Create(new()
      {
        Id = job.Uuid, Collection = job.Collection
      });

      // Fan out to subtasks
      foreach (var subnode in subnodes)
      {
        await relaySubTasks.Create(job.Uuid, subnode.Id);

        // TODO: Rabbit
      }
    }
  }
}
