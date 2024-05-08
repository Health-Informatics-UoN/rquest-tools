using System.Text;
using System.Text.Json;
using Flurl;
using Microsoft.Extensions.Options;
using RquestBridge.Config;
using RquestBridge.Models;

namespace RquestBridge.Services;

public class HutchApiClient(HttpClient client,
  IOptions<HutchAgentOptions> hutchAgentOptions,
  ILogger<HutchApiClient> logger, IOptionsSnapshot<MinioOptions> minioOptions,
  IOptions<HutchDatabaseConnectionDetails> hutchDbAccess)
{
  private readonly HutchAgentOptions _hutchAgentOptions = hutchAgentOptions.Value;

  private readonly HutchDatabaseConnectionDetails _hutchDbAccess = hutchDbAccess.Value;

  // send externalised version of Minio options
  private readonly MinioOptions _minioOptions = minioOptions.Get(MinioOptions.External);

  private StringContent AsHttpJsonString<T>(T value)
    => new StringContent(
      JsonSerializer.Serialize(value),
      Encoding.UTF8,
      "application/json");

  public async Task HutchEndpointPost(string jobId)
  {
    var requestUri = Url.Combine(_hutchAgentOptions.Host,
      _hutchAgentOptions.EndpointBase,
      _hutchAgentOptions.SubmitJobEndpoint
    );
    var payload = new HutchJob()
    {
      SubId = jobId,
      CrateSource = new FileStorageDetails()
      {
        AccessKey = _minioOptions.AccessKey,
        Bucket = _minioOptions.Bucket,
        Path = jobId + ".zip",
        SecretKey = _minioOptions.SecretKey,
        Host = _minioOptions.Host,
        Secure = _minioOptions.Secure
      }
    };

// conditionally pass on db details?
    // TODO: much more robust version needed
    if (!string.IsNullOrWhiteSpace(_hutchDbAccess.Hostname) && !string.IsNullOrWhiteSpace(_hutchDbAccess.Database))
    {
      payload.DataAccess = _hutchDbAccess;
    }

    // POST to HutchAgent
    var response = await client.PostAsync(requestUri, AsHttpJsonString(payload));
    logger.LogInformation("{Status}", response.StatusCode.ToString());
  }
}
