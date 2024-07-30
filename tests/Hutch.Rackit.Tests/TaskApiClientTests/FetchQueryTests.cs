using System.Net;
using System.Text.Json;
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
  public async void ReturnsNullIfNoJobs(string jobTypeSuffix)
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

  [Theory]
  [InlineData(".a")] // Availability Queries
  [InlineData(".b")] // Distribution Queries
  public async void UsesServiceConfiguredOptionsByDefault(string jobTypeSuffix)
  {
    var http = new MockHttpMessageHandler();

    // This mock will only respond correctly if all the correct configuration is used
    // This also tests that basic auth credentials are being passed
    http.When(
      Url.Combine(_configuredOptions.BaseUrl,
      _fetchQueryEndpoint,
      _configuredOptions.CollectionId + jobTypeSuffix)
      )
      .WithHeaders("Authorization", "Basic " + _configuredOptions.BasicCredentials)
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

    // The assertion is kind of irrelevant; we want to get here without exceptions
    // but we may as well confirm that the success behaviour is as expected
    Assert.Null(result);
  }

  [Theory]
  [InlineData(".a")] // Availability Queries
  [InlineData(".b")] // Distribution Queries
  public async void UsesOverrideOptions(string jobTypeSuffix)
  {
    var http = new MockHttpMessageHandler();

    var overrideOptions = new ApiClientOptions
    {
      BaseUrl = "https://api.different.com",
      CollectionId = "collection-xyz",
      Username = "override",
      Password = "xyz789",
    };

    // This mock will only respond correctly if all the correct configuration is used
    http.When(
      Url.Combine(overrideOptions.BaseUrl,
      _fetchQueryEndpoint,
      overrideOptions.CollectionId + jobTypeSuffix)
      )
      .WithHeaders("Authorization", "Basic " + overrideOptions.BasicCredentials)
      .Respond(HttpStatusCode.NoContent);

    var client = new TaskApiClient(
      http.ToHttpClient(),
      Options.Create(_configuredOptions),
      _logger);

    TaskApiBaseResponse? result = jobTypeSuffix switch
    {
      ".a" => await client.FetchQuery<AvailabilityQuery>(overrideOptions),
      ".b" => await client.FetchQuery<DistributionQuery>(overrideOptions),
      _ => null
    };

    // The assertion is kind of irrelevant; we want to get here without exceptions
    // but we may as well confirm that the success behaviour is as expected
    Assert.Null(result);
  }

  // FetchQuery returns requested response type
  [Fact]
  public async void AvailabilityQueryReturnsAvailabilityQuery()
  {
    var response = new AvailabilityQuery
    {
      Collection = _configuredOptions.CollectionId,
      Uuid = "86",
      Owner = "user1",
      Cohort = new()
      {
        Combinator = "AND",
        Groups = []
      }
    };

    var http = new MockHttpMessageHandler();

    http.When(
      Url.Combine(_configuredOptions.BaseUrl,
      _fetchQueryEndpoint,
      _configuredOptions.CollectionId + ".a")
      )
      .Respond("application/json", JsonSerializer.Serialize(response));

    var client = new TaskApiClient(
      http.ToHttpClient(),
      Options.Create(_configuredOptions),
      _logger);

    var result = await client.FetchQuery<AvailabilityQuery>();

    Assert.IsType<AvailabilityQuery>(result);
  }
  [Fact]
  public async void DistributionQueryReturnsDistributionQuery()
  {
    var response = new DistributionQuery
    {
      Analysis = "DISTRIBUTION",
      Code = "GENERIC",
      Collection = _configuredOptions.CollectionId,
      Uuid = "86",
      Owner = "user1",
    };

    var http = new MockHttpMessageHandler();

    http.When(
      Url.Combine(_configuredOptions.BaseUrl,
      _fetchQueryEndpoint,
      _configuredOptions.CollectionId + ".b")
      )
      .Respond("application/json", JsonSerializer.Serialize(response));

    var client = new TaskApiClient(
      http.ToHttpClient(),
      Options.Create(_configuredOptions),
      _logger);

    var result = await client.FetchQuery<DistributionQuery>();

    Assert.IsType<DistributionQuery>(result);
  }
}
