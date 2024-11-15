using Hutch.Rackit;
using Hutch.Rackit.TaskApi.Models;
using Hutch.Relay.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Hutch.Relay.Services.Hosted;

/// <summary>
/// This is a hosted service for monitoring RelayTasks
/// </summary>
public class TaskCompletionHostedService(
  IServiceScopeFactory serviceScopeFactory,
  ILogger<TaskCompletionHostedService> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      using var scope = serviceScopeFactory.CreateScope();
      var relayTaskService = scope.ServiceProvider.GetRequiredService<IRelayTaskService>();
      var taskResultsService = scope.ServiceProvider.GetRequiredService<ResultsService>();

      logger.LogInformation("Checking for incomplete Tasks...");
      var incompleteTasks = await relayTaskService.ListIncomplete();
      foreach (var task in incompleteTasks)
      {
        logger.LogInformation("Incomplete task:{Task}", task.Id);
        var timeInterval = DateTimeOffset.UtcNow.Subtract(task.CreatedAt);
        if (timeInterval > TimeSpan.FromMinutes(4.5))
        {
          // get subtasks 
          var subTasks = await relayTaskService.ListSubTasks(task.Id, incompleteOnly: false);
          // aggregate subtasks
          var aggregateCount = taskResultsService.AggregateResults(subTasks.ToList());
          //Post back to api 

          //Set task as complete
          await relayTaskService.SetComplete(task.Id);
        }
      }

      await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
    }
  }
}
