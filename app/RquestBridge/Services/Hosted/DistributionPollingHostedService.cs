using Microsoft.Extensions.Options;
using RquestBridge.Config;

namespace RquestBridge.Services.Hosted;

public class DistributionPollingHostedService(
  ILogger<DistributionPollingHostedService> logger,
  IServiceProvider serviceProvider,
  IOptions<RQuestPollingOptions> config,
  IOptions<RQuestOptions> rQuestOptions
) : BackgroundService
{
  private int _executionCount;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("RQuest Polling started");

    if (config.Value.PollingInterval < 0) return;

    while (!stoppingToken.IsCancellationRequested)
    {
      var executionTask = TriggerActivitySourcePolling();

      await Task.Delay(TimeSpan.FromSeconds(config.Value.PollingInterval), stoppingToken);
    }
  }

  private async Task TriggerActivitySourcePolling()
  {
    var count = Interlocked.Increment(ref _executionCount);

    logger.LogDebug(
      "{Service} is working. Count: {Count}", nameof(DistributionPollingHostedService), count);

    using var executionScope = serviceProvider.CreateScope();

    // TODO use another worker service with DI instead of Service Locator? 

    var rQuestPoller = executionScope.ServiceProvider.GetRequiredService<RQuestDistributionPollingService>();

    List<Task> pollTasks = new();

    pollTasks.Add(rQuestPoller.Poll(rQuestOptions.Value));
    await Task.WhenAll(pollTasks);
  }

  public override async Task StopAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("Activity Source Polling stopping");

    await base.StopAsync(stoppingToken);
  }
}
