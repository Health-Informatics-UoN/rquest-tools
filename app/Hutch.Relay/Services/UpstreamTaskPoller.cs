using Hutch.Rackit;
using Hutch.Rackit.TaskApi;
using Hutch.Rackit.TaskApi.Models;
using Hutch.Relay.Services.Contracts;
using Microsoft.AspNetCore.Identity;
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
  RelaySubTaskService relaySubTasks,
  IRelayTaskQueue queues) : BackgroundService
{
  protected override Task ExecuteAsync(CancellationToken stoppingToken)
  {
    // We need to simultaneously poll against all supported task queues in an upstream system
    // TODO: this may become configurable to support upstream unified queues e.g. in Relay

    // Start polling for job types:
    var availabilityQueries = upstreamTasks.PollJobQueue<AvailabilityJob>(options.Value, stoppingToken);
    var collectionAnalyses = upstreamTasks.PollJobQueue<CollectionAnalysisJob>(options.Value, stoppingToken);

    // TODO: "Type C" var cohortAnalyses = upstreamTasks.PollJobQueue<>(options.Value, stoppingToken);

    // start parallel handler threads
    return Task.WhenAll(
      HandleTasksFound(availabilityQueries, stoppingToken),
      HandleTasksFound(collectionAnalyses, stoppingToken));
  }

  private async Task HandleTasksFound<T>(IAsyncEnumerable<T> jobs, CancellationToken cancellationToken)
    where T : TaskApiBaseResponse
  {
    await foreach (var job in jobs.WithCancellation(cancellationToken))
    {
      logger.LogInformation("Task handled: ({Type}) {Id}", typeof(T).Name, job.Uuid);

      var subnodes = (await subNodes.List()).ToList();
      if (subnodes.Count == 0) return;

      // Create a parent task
      var relayTask = await relayTasks.Create(new()
      {
        Id = job.Uuid, Collection = job.Collection
      });

      // Fan out to subtasks
      foreach (var subnode in subnodes)
      {
        var subTask = await relaySubTasks.Create(relayTask.Id, subnode.Id);

        // Update the job for the target subnode
        job.Uuid = subTask.Id.ToString();
        job.Collection = subnode.Id.ToString();
        job.Owner = subnode.Owner;

        // TODO: Rabbit
        await queues.Send(subnode.Id.ToString(), job);
      }
    }
  }
}
