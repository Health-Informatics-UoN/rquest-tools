using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Flurl;
using Hutch.Rackit.Models.TaskApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hutch.Rackit;

public class TaskApiClient(
  HttpClient client,
  IOptions<ApiClientOptions> configuredOptions,
  ILogger<TaskApiClient> logger
)
{
  /// <summary>
  /// Default options for the service as configured
  /// </summary>
  private readonly ApiClientOptions _options = configuredOptions.Value; // TODO: how to handle required?

  public async Task<T?> FetchQuery<T>() where T : TaskApiBaseResponse, new()
    => await FetchQuery<T>(_options);

  public async Task<T?> FetchQuery<T>(ApiClientOptions options) where T : TaskApiBaseResponse, new()
  {
    var typeSuffix = new T() switch
    {
      AvailabilityQuery => JobTypeSuffixes.AvailabilityQuery,
      DistributionQuery => JobTypeSuffixes.Distribution,
      _ => throw new RackitApiClientException($"Unexpected Task API Response type requested: {typeof(T)}.")
    };

    var requestUrl = Url.Combine(
      options.BaseUrl,
      TaskApiEndpoints.Base,
      TaskApiEndpoints.FetchQuery,
      options.CollectionId + typeSuffix);

    using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", options.BasicCredentials);

    logger.LogDebug(JsonSerializer.Serialize(request.Headers));

    var result = await client.SendAsync(request);

    if (result.IsSuccessStatusCode)
    {
      if (result.StatusCode == HttpStatusCode.NoContent)
      {
        logger.LogInformation("No Query Jobs waiting for {CollectionId}", options.CollectionId);
        return null;
      }

      try
      {
        return await result.Content.ReadFromJsonAsync<T>();
      }
      catch (JsonException e)
      {
        logger.LogError(e, "Invalid Response Format from Fetch Query Endpoint");

        var body = await result.Content.ReadAsStringAsync();
        logger.LogDebug("Invalid Response Body: {Body}", body);

        throw;
      }
    }
    else
    {
      var body = await result.Content.ReadAsStringAsync();
      logger.LogError("Fetch Query Endpoint Request failed: {StatusCode}", result.StatusCode);
      logger.LogDebug("Failure Response Body:\n{Body}", body);
      throw new RackitApiClientException($"Fetch Query Endpoint Request failed: {result.StatusCode}");
    }
  }
}

internal static class JobTypeSuffixes
{
  public const string AvailabilityQuery = ".a";
  public const string AnalyticsGenePhewas = ".b";
  public const string Distribution = ".b";
  public const string AnalyticsGwas = ".c";
  public const string AnalyticsGwasQuantitiveTrait = ".c";
  public const string AnalyticsBurdenTest = ".c";
}


internal static class TaskApiEndpoints
{
  public const string Base = "link_connector_api/task";

  public const string QueueStatus = "queue";

  public const string FetchQuery = "nextjob";

  public const string SubmitResult = "result";
}
