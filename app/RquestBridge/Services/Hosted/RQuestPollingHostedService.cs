using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RquestBridge.Config;
using RquestBridge.Models;

namespace RquestBridge.Services.Hosted;

public class RQuestPollingHostedService : BackgroundService
{
  private readonly ILogger<RQuestPollingHostedService> _logger;
  private readonly IServiceProvider _serviceProvider;
  private readonly RQuestPollingOptions _config;
  private readonly RQuestOptions _rQuestOptions;
  private int _executionCount;

  public RQuestPollingHostedService(
    ILogger<RQuestPollingHostedService> logger,
    IServiceProvider serviceProvider,
    IOptions<RQuestPollingOptions> config, 
    IOptions<RQuestOptions> rQuestOptions)
  {
    _logger = logger;
    _serviceProvider = serviceProvider;
    _rQuestOptions = rQuestOptions.Value;
    _config = config.Value;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("RQuest Polling started");

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

    _logger.LogDebug(
      "{Service} is working. Count: {Count}", nameof(RQuestAvailabilityPollingService), count);

    using var executionScope = _serviceProvider.CreateScope();

    // TODO use another worker service with DI instead of Service Locator? 

    var rQuestPoller = executionScope.ServiceProvider.GetRequiredService<RQuestAvailabilityPollingService>();

    List<Task> pollTasks = new();

    pollTasks.Add(rQuestPoller.Poll(_rQuestOptions));
    await Task.WhenAll(pollTasks);
  }

  public override async Task StopAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Activity Source Polling stopping");

    await base.StopAsync(stoppingToken);
  }
}
