using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RquestBridge.Config;

namespace RquestBridge.Services.Hosted;

public class RQuestPollingHostedService(ILogger<RQuestPollingHostedService> logger,
    IServiceProvider serviceProvider,
    IOptions<RQuestPollingOptions> config,
    IOptions<RQuestOptions> rQuestOptions)
  : BackgroundService
{
  private readonly RQuestPollingOptions _config = config.Value;
  private readonly RQuestOptions _rQuestOptions = rQuestOptions.Value;
  private int _executionCount;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("RQuest Polling started");

    if (_config.PollingInterval < 0) return;

    while (!stoppingToken.IsCancellationRequested)
    {
      var executionTask = TriggerActivitySourcePolling();

      await Task.Delay(TimeSpan.FromSeconds(_config.PollingInterval), stoppingToken);
    }
  }

  private async Task TriggerActivitySourcePolling()
  {
    var count = Interlocked.Increment(ref _executionCount);

    logger.LogDebug(
      "{Service} is working. Count: {Count}", nameof(RQuestAvailabilityPollingService), count);

    using var executionScope = serviceProvider.CreateScope();

    // TODO use another worker service with DI instead of Service Locator? 

    var rQuestPoller = executionScope.ServiceProvider.GetRequiredService<RQuestAvailabilityPollingService>();

    List<Task> pollTasks = new();

    pollTasks.Add(rQuestPoller.Poll(_rQuestOptions));
    await Task.WhenAll(pollTasks);
  }

  public override async Task StopAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("Activity Source Polling stopping");

    await base.StopAsync(stoppingToken);
  }
}
