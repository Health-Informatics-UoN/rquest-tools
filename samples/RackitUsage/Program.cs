using System.Text.Json;
using Hutch.Rackit;
using Hutch.Rackit.TaskApi;
using Hutch.Rackit.TaskApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// Control sample app behaviour paths
var doPolling = true;

// Set up the client's dependencies so we can pass them in
var options = Options.Create(new ApiClientOptions
{
  // Fill in your connection details
});

var httpClient = new HttpClient(); // Don't just do this in a real app

using var factory = LoggerFactory.Create(o =>
{
  o.AddConsole();
  o.SetMinimumLevel(LogLevel.Debug);
});
var logger = factory.CreateLogger<TaskApiClient>();


logger.LogInformation(
  "Default Options: {Options}",
  JsonSerializer.Serialize(options.Value, new JsonSerializerOptions { WriteIndented = true }));

// Do some API stuff!
var client = new TaskApiClient(httpClient, options, logger);

async Task HandleAvailabilityJob(AvailabilityJob job)
{
  logger.LogInformation("Found Availability job: {Job}", JsonSerializer.Serialize(job));

  await Task.Delay(10000); // Wait while we "query". Nice for the GUI to show "sent to client" vs "job done"

  await client.SubmitResultAsync(job.Uuid, new()
  {
    Uuid = job.Uuid,
    CollectionId = job.Collection,
    Status = "OK",
    Message = "Results",
    Results = new()
    {
      Count = 123,
      DatasetCount = 1,
      Files = []
    }
  });

  logger.LogInformation("Response sent for Availability job: {JobId}", job.Uuid);
}

if (doPolling)
{
  // start polling availability jobs
  logger.LogInformation("Checking for Availability jobs, for 1 minute");

  // set a timer to cancel polling after 1 min
  using var cts = new CancellationTokenSource();
  var timer = new System.Timers.Timer(TimeSpan.FromMinutes(1))
  {
    AutoReset = false
  };
  timer.Elapsed += (s, e) => { cts.Cancel(); };
  timer.Start();

  // respond to polling
  await foreach (var job in client.PollJobQueue<AvailabilityJob>().WithCancellation(cts.Token))
  {
    await HandleAvailabilityJob(job);
  }
}
else
{

  // Fetch an Availability job for the configured collection
  logger.LogInformation("Checking for Availability jobs...");

  var availabilityJob = await client.FetchNextJobAsync<AvailabilityJob>();

  // Handle the job if there is one, by returning a stock result
  if (availabilityJob is not null) await HandleAvailabilityJob(availabilityJob);
  else logger.LogInformation("No Availability jobs waiting!");


  // Fetch a CollectionAnalysis job for the configured collection
  logger.LogInformation("Checking for Collection Analysis jobs...");

  var analysisJob = await client.FetchNextJobAsync<CollectionAnalysisJob>();

  // Handle the job if there is one, by returning a stock result
  if (analysisJob is not null)
  {
    logger.LogInformation("Found Collection Analysis job: {Job}", JsonSerializer.Serialize(analysisJob));

    await Task.Delay(10000); // Wait while we "query". Nice for the GUI to show "sent to client" vs "job done"

    var codeDistributionResult = new QueryResult
    {
      Count = 1,
      DatasetCount = 1,
      Files = [
        new ResultFile
        {
          FileDescription = "code.distribution analysis results",
        }
        .WithData( // encodes the data and sets FileData and FileSize properties for us
          """
          BIOBANK	CODE	COUNT	DESCRIPTION	MIN	Q1	MEDIAN	MEAN	Q3	MAX	ALTERNATIVES	DATASET	OMOP	OMOP_DESCR	CATEGORY
        <collection id>	OMOP:443614	123	nan	nan	nan	nan	nan	nan	nan	nan	nan	443614	Chronic kidney disease stage 1	Condition
          """)
      ]
    };

    var unhandledResults = new QueryResult
    {
      Count = 0,
      DatasetCount = 0,
      Files = []
    };

    await client.SubmitResultAsync(analysisJob.Uuid, new()
    {
      Uuid = analysisJob.Uuid,
      CollectionId = analysisJob.Collection,
      Status = "OK",
      Message = "Results",
      Results = analysisJob.Analysis switch
      {
        AnalysisType.Distribution => analysisJob.Code switch
        {
          DistributionCode.Generic => codeDistributionResult,
          _ => unhandledResults
        },
        _ => unhandledResults
      }
    });

    logger.LogInformation("Response sent for job: {JobId}", analysisJob.Uuid);
  }
  else
  {
    logger.LogInformation("No Collection Analysis jobs waiting!");
  }
}
