using Hutch.Relay.Services.Contracts;

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
      logger.LogInformation("Checking for incomplete Tasks...");
      var incompleteTasks = await relayTaskService.ListIncomplete();
      foreach (var task in incompleteTasks)
      {
        logger.LogInformation("Incomplete task:{Task}", task.Id);
        var timeInterval = DateTimeOffset.UtcNow.Subtract(task.CreatedAt);
        if (timeInterval > TimeSpan.FromMinutes(4.5))
        {
          //Post back to api 
          // 

          //Set task as complete
          await relayTaskService.SetComplete(task.Id);
        }
      }

      await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
    }
  }
}
