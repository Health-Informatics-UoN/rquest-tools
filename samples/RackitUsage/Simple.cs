using Hutch.Rackit;
using Hutch.Rackit.TaskApi;
using Hutch.Rackit.TaskApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RackitUsage;

public class Simple
{
  private readonly ILogger<Simple> _logger;
  private readonly TaskApiClient _taskApiClient;
  private readonly JobHandler _jobHandler;
  
  public Simple(IOptions<ApiClientOptions> options)
  {
    // new up our own dependencies in the right order
    
    var httpClient = new HttpClient(); // Don't just do this in a real app
    using var factory = LoggerFactory.Create(o =>
    {
      o.AddConsole();
      o.SetMinimumLevel(LogLevel.Debug);
    });

    _logger = factory.CreateLogger<Simple>();

    _taskApiClient = new TaskApiClient(httpClient, options, factory.CreateLogger<TaskApiClient>());

    _jobHandler = new JobHandler(factory.CreateLogger<JobHandler>(), _taskApiClient);
  }

  public async Task PollAvailability(TimeSpan pollFor)
  {
    // start polling availability jobs
    _logger.LogInformation("Checking for Availability jobs, for the configured time");

    // set a timer to cancel polling after  the configured time
    var cts = new CancellationTokenSource();
    var timer = new System.Timers.Timer(pollFor)
    {
      AutoReset = false
    };
    timer.Elapsed += (s, e) => { cts.Cancel(); cts.Dispose(); };
    timer.Start();

    // respond to polling
    var jobs = _taskApiClient.PollJobQueue<AvailabilityJob>()
      .WithCancellation(cts.Token);
    
    await foreach (var job in jobs)
    {
      await _jobHandler.HandleAvailabilityJob(job);
    }
  }

  public async Task CheckAllQueuesOnce()
  {
    // 1. Fetch an Availability job for the configured collection
    _logger.LogInformation("Checking for Availability jobs...");

    var availabilityJob = await _taskApiClient.FetchNextJobAsync<AvailabilityJob>();

    // Handle the job if there is one, by returning a stock result
    if (availabilityJob is not null) await _jobHandler.HandleAvailabilityJob(availabilityJob);

    else _logger.LogInformation("No Availability jobs waiting!");


    
    
    // 2. Fetch a CollectionAnalysis job for the configured collection
    _logger.LogInformation("Checking for Collection Analysis jobs...");

    var analysisJob = await _taskApiClient.FetchNextJobAsync<CollectionAnalysisJob>();

    // Handle the job if there is one, by returning a stock result
    if (analysisJob is not null) await _jobHandler.HandleCollectionAnalysisJob(analysisJob);
    else _logger.LogInformation("No Collection Analysis jobs waiting!");
  }
}
