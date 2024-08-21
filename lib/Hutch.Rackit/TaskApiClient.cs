using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Flurl;
using Hutch.Rackit.Models.TaskApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hutch.Rackit;

/// <summary>
/// A client for interacting with the Task Api endpoints
/// </summary>
public class TaskApiClient(
  HttpClient client,
  IOptions<ApiClientOptions> configuredOptions,
  ILogger<TaskApiClient> logger
)
{
  /// <summary>
  /// Default options for the service as configured
  /// </summary>
  public ApiClientOptions Options = configuredOptions.Value;

  /// <summary>
  /// Encode a username and password into the combined base64 format
  /// expected for a Basic Authentication header.
  /// </summary>
  /// <param name="username">The username to be included in the encoded result</param>
  /// <param name="password">The password to use in the encoded result</param>
  /// <returns>A base64 string of the input parameters</returns>
  internal static string EncodeCredentialsForBasicAuth(string username, string password)
    => Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));

  /// <summary>
  /// Calls <see cref="FetchQuery"/>, optionally with the options specified in the provided object.
  /// 
  /// Any missing options will fall back to the service's default configured options.
  /// </summary>
  /// <typeparam name="T">The type of query (and response model to be returned)</typeparam>
  /// <param name="options">The options specified to override the defaults</param>
  /// <returns>A model of the requested query type if one was found; <c>null</c> if not.</returns>
  /// <exception cref="ArgumentException">A required option is missing because it wasn't provided and is not present in the service defaults</exception>
  public async Task<T?> FetchQuery<T>(ApiClientOptions? options = null) where T : TaskApiBaseResponse, new()
  {
    static string exceptionMessage(string propertyName)
      => $"The property '{propertyName}' was not specified, and no default is available to fall back to.";

    return await FetchQuery<T>(
      options?.BaseUrl ?? Options.BaseUrl ?? throw new ArgumentException(exceptionMessage(nameof(options.BaseUrl))),
      options?.CollectionId ?? Options.CollectionId ?? throw new ArgumentException(exceptionMessage(nameof(options.CollectionId))),
      options?.Username ?? Options.Username ?? throw new ArgumentException(exceptionMessage(nameof(options.Username))),
      options?.Password ?? Options.Password ?? throw new ArgumentException(exceptionMessage(nameof(options.Password)))
    );
  }

  /// <summary>
  /// Fetch the next query, if any, of the requested type.
  /// </summary>
  /// <typeparam name="T">The type of query (and response model to be returned)</typeparam>
  /// <param name="baseUrl">Base URL of the API instance to connect to.</param>
  /// <param name="collectionId">Collection ID to fetch query for.</param>
  /// <param name="username">Username to use when connecting to the API.</param>
  /// <param name="password">Password to use when connecting to the API.</param>
  /// <returns>A model of the requested query type if one was found; <c>null</c> if not.</returns>
  /// <exception cref="RackitApiClientException">An unknown type was requested, or an otherwise unexpected error occurred while interacting with the API.</exception>
  public async Task<T?> FetchQuery<T>(string baseUrl, string collectionId, string username, string password) where T : TaskApiBaseResponse, new()
  {
    var typeSuffix = new T() switch
    {
      AvailabilityQuery => JobTypeSuffixes.AvailabilityQuery,
      DistributionQuery => JobTypeSuffixes.Distribution,
      _ => throw new RackitApiClientException($"Unexpected Task API Response type requested: {typeof(T)}.")
    };

    var requestUrl = Url.Combine(
      baseUrl,
      TaskApiEndpoints.Base,
      TaskApiEndpoints.FetchQuery,
      collectionId + typeSuffix);

    // TODO: reusable request helper?
    using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

    request.Headers.Authorization = new AuthenticationHeaderValue(
      "Basic",
      EncodeCredentialsForBasicAuth(username, password));

    var result = await client.SendAsync(request);

    if (result.IsSuccessStatusCode)
    {
      if (result.StatusCode == HttpStatusCode.NoContent)
      {
        logger.LogInformation("No Query Jobs waiting for {CollectionId}", collectionId);
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

  private static StringContent AsHttpJsonString<T>(T value)
      => new(
        JsonSerializer.Serialize(value),
        Encoding.UTF8,
        "application/json");

  /// <summary>
  /// Post to the Results endpoint, and handle the response correctly.
  /// </summary>
  /// <param name="jobId">Job ID to submit results for.</param>
  /// <param name="result">The results to submit.</param>
  /// <param name="options">The options specified to override the defaults</param>
  /// <exception cref="ArgumentException">A required option is missing because it wasn't provided and is not present in the service defaults</exception>
  public async Task SubmitResult(string jobId, Result result, ApiClientOptions? options = null)
  {
    static string exceptionMessage(string propertyName)
      => $"The property '{propertyName}' was not specified, and no default is available to fall back to.";

    await SubmitResult(
      options?.BaseUrl ?? Options.BaseUrl ?? throw new ArgumentException(exceptionMessage(nameof(options.BaseUrl))),
      options?.CollectionId ?? Options.CollectionId ?? throw new ArgumentException(exceptionMessage(nameof(options.CollectionId))),
      options?.Username ?? Options.Username ?? throw new ArgumentException(exceptionMessage(nameof(options.Username))),
      options?.Password ?? Options.Password ?? throw new ArgumentException(exceptionMessage(nameof(options.Password))),
      jobId,
      result
    );
  }

  /// <summary>
  /// Post to the Results endpoint, and handle the response correctly.
  /// </summary>
  /// <param name="baseUrl">Base URL of the API instance to connect to.</param>
  /// <param name="collectionId">Collection ID to submit results for.</param>
  /// <param name="username">Username to use when connecting to the API.</param>
  /// <param name="password">Password to use when connecting to the API.</param>
  /// <param name="jobId">Job ID to submit results for.</param>
  /// <param name="result">The results to submit.</param>
  /// <exception cref="RackitApiClientException">An unsuccessful response was received from the remote Task API.</exception>
  public async Task SubmitResult(string baseUrl, string collectionId, string username, string password, string jobId, Result result)
  {
    var requestUrl = Url.Combine(
      baseUrl,
      TaskApiEndpoints.Base,
      TaskApiEndpoints.SubmitResult,
      jobId,
      collectionId);

    using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

    request.Headers.Authorization = new AuthenticationHeaderValue(
      "Basic",
      EncodeCredentialsForBasicAuth(username, password));

    request.Content = AsHttpJsonString(result);

    var response = await client.SendAsync(request);

    var body = await response.Content.ReadAsStringAsync();

    if (body != "Job saved" || !response.IsSuccessStatusCode)
    {
      const string message = "Unsuccessful Response from Submit Results Endpoint";
      logger.LogError(message);
      logger.LogDebug("Response Body: {Body}", body);

      throw new RackitApiClientException(message);
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
