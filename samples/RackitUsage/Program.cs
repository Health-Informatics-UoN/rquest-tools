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

var result = await client.FetchQuery<AvailabilityQuery>();

Console.WriteLine(result is not null
  ? JsonSerializer.Serialize(result)
  : "No query jobs waiting!");
