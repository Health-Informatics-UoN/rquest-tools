using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using RquestBridge.Config;
using RquestBridge.Constants;
using RquestBridge.Dto;
using RquestBridge.Dto.RQuestResults;

namespace RquestBridge.Services
{
  public class RQuestTaskApiClient
  {
    private readonly RQuestTaskApiOptions _apiOptions;
    private readonly HttpClient _client;
    private readonly ILogger<RQuestTaskApiClient> _logger;
    private readonly RQuestOptions _rQuestOptions;

    public RQuestTaskApiClient(
      HttpClient client,
      ILogger<RQuestTaskApiClient> logger,
      IOptions<RQuestTaskApiOptions> apiOptions, IOptions<RQuestOptions> rQuestOptions)
    {
      _client = client;
      _logger = logger;
      _rQuestOptions = rQuestOptions.Value;
      _apiOptions = apiOptions.Value;

      // TODO: credentials in future will be per Activity Source, so won't be set as default
      string credentials = _apiOptions.Username + ":" + _apiOptions.Password;
      var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
      _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);
    }

    /// <summary>
    /// Serialize a value to a JSON string, and provide HTTP StringContent
    /// for it with a media type of "application/json"
    /// </summary>
    /// <param name="value"></param>
    /// <returns>HTTP StringContent with the value serialized to JSON and a media type of "application/json"</returns>
    [Obsolete]
    private StringContent AsHttpJsonString<T>(T value)
      => new StringContent(
        JsonSerializer.Serialize(value),
        Encoding.UTF8,
        "application/json");

    /// <summary>
    /// Try and get a job for a collection
    /// </summary>
    /// <param name="rQuestOptions"> RQuestOptions</param>
    /// <returns>A Task DTO containing a Query to run, or null if none are waiting</returns>
    public async Task<T?> FetchQuery<T>(RQuestOptions rQuestOptions) where T : class, new()
    {
      var typeSuffix = new T() switch
      {
        AvailabilityQuery _ => RQuestJobTypeSuffixes.AvailabilityQuery,
        DistributionQuery _ => RQuestJobTypeSuffixes.Distribution,
        _ => string.Empty
      };
      var requestUri = Url.Combine(
        rQuestOptions.Host,
        _apiOptions.EndpointBase,
        _apiOptions.FetchQueryEndpoint,
        rQuestOptions.CollectionId + typeSuffix);
      var result = await _client.GetAsync(
        requestUri);

      if (result.IsSuccessStatusCode)
      {
        if (result.StatusCode == HttpStatusCode.NoContent)
        {
          _logger.LogInformation(
            "No Query Jobs waiting for {CollectionId}",
            rQuestOptions.CollectionId);
          return null;
        }

        try
        {
          var job = await result.Content.ReadFromJsonAsync<T>();
          return job;
        }
        catch (JsonException e)
        {
          _logger.LogError(e, "Invalid Response Format from Fetch Query Endpoint");

          var body = await result.Content.ReadAsStringAsync();
          _logger.LogDebug("Invalid Response Body: {Body}", body);

          throw;
        }
      }
      else
      {
        _logger.LogError("Fetch Query Endpoint Request failed: {StatusCode}", result.StatusCode);
        throw new ApplicationException($"Fetch Query Endpoint Request failed: {result.StatusCode}");
      }
    }

    /// <summary>
    /// Post to the Results endpoint, and handle the response correctly
    /// </summary>
    /// <param name="jobId">Job ID</param>
    /// <param name="result">Results with Count</param>
    public async Task ResultsEndpointPost(string jobId, RquestQueryResult result)
    {
      var requestUri = Url.Combine(
        _rQuestOptions.Host,
        _apiOptions.EndpointBase,
        _apiOptions.SubmitResultEndpoint,
        jobId,
        _rQuestOptions.CollectionId);

      var response = await requestUri.WithBasicAuth(_apiOptions.Username, _apiOptions.Password).PostJsonAsync(result);

      var body = await response.ResponseMessage.Content.ReadAsStringAsync();

      if (body != "Job saved" && !response.ResponseMessage.IsSuccessStatusCode)
      {
        const string message = "Unsuccessful Response from Submit Results Endpoint";
        _logger.LogError(message);
        _logger.LogDebug("Response Body: {Body}", body);

        throw new ApplicationException(message);
      }
    }
  }
}
