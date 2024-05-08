namespace RquestBridge.Config;

public class MinioOptions
{
  // See https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-8.0#named-options-support-using-iconfigurenamedoptions
  /// <summary>
  /// <para>The section header for system Minio settings.</para>
  /// <para>
  /// If you are deploying to a docker compose network, the host should be the name of the Minio service
  /// in the docker-compose.yml.
  /// </para>
  /// </summary>
  public const string System = "System";

  /// <summary>
  /// <para>The section header for telling external systems how to talk to Minio.</para>
  /// <para>This is useful when your Minio runs in a docker compose network alongside <c>RquestBridge</c></para>
  /// </summary>
  public const string External = "External";

  public string Host { get; set; } = "localhost:9000";
  public string AccessKey { get; set; } = string.Empty;
  public string SecretKey { get; set; } = string.Empty;
  public bool Secure { get; set; } = true;
  public string Bucket { get; set; } = string.Empty;
}
