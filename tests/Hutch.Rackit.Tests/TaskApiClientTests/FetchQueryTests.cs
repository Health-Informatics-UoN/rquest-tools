using System.Net;
using Flurl;
using Hutch.Rackit.Models.TaskApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;

namespace Hutch.Rackit.Tests.TaskApiClientTests;

public class FetchQueryTests
{
  private static readonly ApiClientOptions _configuredOptions = new()
  {
    BaseUrl = "http://example.com",
    CollectionId = "collection-001",
    Username = "api-user",
    Password = "abc123"
  };

  private static readonly string _fetchQueryEndpoint =
    "link_connector_api/task/nextjob";

  private readonly ILogger<TaskApiClient> _logger = Mock.Of<ILogger<TaskApiClient>>();

  [Theory]
  [InlineData(".a")] // Availability Queries
  [InlineData(".b")] // Distribution Queries
  // TODO: other types?
  public async void FetchQueryReturnsNullIfNoJobs(string jobTypeSuffix)
  {
    var http = new MockHttpMessageHandler();

    http.When(
      Url.Combine(_configuredOptions.BaseUrl,
      _fetchQueryEndpoint,
      _configuredOptions.CollectionId + jobTypeSuffix)
      )
      .Respond(HttpStatusCode.NoContent);

    var client = new TaskApiClient(
      http.ToHttpClient(),
      Options.Create(_configuredOptions),
      _logger);

    TaskApiBaseResponse? result = jobTypeSuffix switch
    {
      ".a" => await client.FetchQuery<AvailabilityQuery>(),
      ".b" => await client.FetchQuery<DistributionQuery>(),
      _ => null
    };

    Assert.Null(result);
  }

  // FetchQuery uses service configured options by default

  // FetchQuery correctly uses specific options instead of service configuration

  // FetchQuery includes auth header

  // FetchQuery uses correct suffix for availability query
  // FetchQuery uses correct suffix for distribution query

  // FetchQuery returns requested response type

  // FetchQuery option builder uses specific options
}
