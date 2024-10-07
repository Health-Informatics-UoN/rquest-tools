// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using Microsoft.Extensions.Hosting;

// namespace Hutch.Rackit.TaskApi;

// [Flags]
// public enum PollingTargetQueues
// {
//   AvailabilityJobs = 1,
//   CollectionAnalysisJobs = 2,
//   CohortAnalysisJobs = 4,
//   AvailabilityAndCollectionAnalysis = AvailabilityJobs | CollectionAnalysisJobs,
//   AvailabilityAndCohortAnalysis = AvailabilityJobs | CohortAnalysisJobs,
//   CollectionAndCohortAnalysis = CollectionAnalysisJobs | CohortAnalysisJobs,
//   All = AvailabilityJobs | CollectionAnalysisJobs | CohortAnalysisJobs,
// }

// public class BackgroundJobQueuePoller(ILogger<BackgroundJobQueuePoller> logger,
//     IServiceProvider serviceProvider,
//     IOptions<ApiClientOptions> options)
//   : BackgroundService
// {
//   // TODO Job Type handling? // Configurable and parallel polling of types IMHO
//   public PollingTargetQueues TargetQueues { get; set; } = PollingTargetQueues.All;

//   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//   {
//     logger.LogInformation("RQuest Polling started");

//     if (options.Value.PollingFrequency < 0) return;

//     while (!stoppingToken.IsCancellationRequested)
//     {
//       // TODO: This is insane, sort of nested looping/polling and error handling...
//       // Instead we should use the basic polling (which breaks on exceptions) in Task Api Client, but restart on exceptions (to a point?)
//       // If we want dynamic fall off, maybe add taht here via custom polling, rather than Task Api Client?
//       // Use Observer Pattern to trigger event handlers when jobs are found
//       // Make configurable via DI extensions
//       var executionTask = TriggerActivitySourcePolling();

//       await Task.Delay(TimeSpan.FromSeconds(options.Value.PollingFrequency), stoppingToken);
//     }
//   }

//   private async Task TriggerActivitySourcePolling()
//   {
//     var count = Interlocked.Increment(ref _executionCount);

//     logger.LogDebug(
//       "{Service} is working. Count: {Count}", nameof(RQuestAvailabilityPollingService), count);

//     using var executionScope = serviceProvider.CreateScope();

//     // TODO use another worker service with DI instead of Service Locator? 

//     var rQuestPoller = executionScope.ServiceProvider.GetRequiredService<RQuestAvailabilityPollingService>();

//     List<Task> pollTasks = new();

//     pollTasks.Add(rQuestPoller.Poll(_rQuestOptions));
//     // Add pollers for each configured job queue?
//     // each `foreach await` could be run on its own thread in parallel,
//     // resolving to a task when cancelled (or completed?) and then we await them all, as below?
//     await Task.WhenAll(pollTasks);
//   }

//   public override async Task StopAsync(CancellationToken stoppingToken)
//   {
//     logger.LogInformation("Activity Source Polling stopping");

//     await base.StopAsync(stoppingToken);
//   }
// }
