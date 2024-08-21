using System.Text.Json;
using Hutch.Rackit;
using Hutch.Rackit.Models.TaskApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

// Fetch a query for the configured collection
var result = await client.FetchQuery<AvailabilityQuery>();

// Handle the query if there is one, by returning a stock result
if (result is not null)
{
  Console.WriteLine(JsonSerializer.Serialize(result));

  await Task.Delay(10000); // Wait while we "query". Nice for the GUI to show "sent to client" vs "job done"

  await client.SubmitResult(result.Uuid, new()
  {
    Uuid = result.Uuid,
    CollectionId = result.Collection,
    Status = "OK",
    Message = "Results",
    Results = new()
    {
      Count = 123,
      DatasetCount = 1,
      Files = []
    }
  });

  Console.WriteLine($"Response sent for job: {result.Uuid}");
}
else
{
  Console.WriteLine("No query jobs waiting!");
}
