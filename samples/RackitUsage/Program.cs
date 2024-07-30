using System.Text.Json;
using Hutch.Rackit;
using Hutch.Rackit.Models.TaskApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var options = Options.Create(new ApiClientOptions
{
  // Fill in your connection details
  BaseUrl = "https://example.com",
  CollectionId = "collection_id",
  Username = "user",
  Password = "password",
});

var httpClient = new HttpClient(); // Don't do this in a real app

using var factory = LoggerFactory.Create(o =>
{
  o.AddConsole();
  o.SetMinimumLevel(LogLevel.Debug);
});
var logger = factory.CreateLogger<TaskApiClient>();

logger.LogInformation("options: {Options}", JsonSerializer.Serialize(options));

var client = new TaskApiClient(httpClient, options, logger);

var result = await client.FetchQuery<AvailabilityQuery>();

Console.WriteLine(result is not null
  ? JsonSerializer.Serialize(result)
  : "No query jobs waiting!");
