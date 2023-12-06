using System.ComponentModel.DataAnnotations;

namespace RquestBridge.Models;

public class FileStorageDetails
{
  /// <summary>
  /// Cloud Storage Host e.g. a Min.io Server. Defaults to preconfigured value if omitted.
  /// </summary>
  public string Host { get; set; } = string.Empty;

  /// <summary>
  /// Defaults to true; should only really be false for development/testing.
  /// </summary>
  public bool Secure { get; set; } = true;

  /// <summary>
  /// Cloud Storage Container name e.g. a Min.io Bucket. Defaults to preconfigured value if omitted.
  /// </summary>
  public string Bucket { get; set; } = string.Empty;

  /// <summary>
  /// Object ID for a single Cloud Storage item in the named <see cref="Bucket"/>,
  /// functionally the "Path" to the stored file.
  /// </summary>
  [Required]
  public string Path { get; set; } = string.Empty;

  /// <summary>
  /// Access Key for Cloud Storage.
  /// </summary>
  public string AccessKey { get; set; } = string.Empty;

  /// <summary>
  /// Secret Key for Cloud Storage.
  /// </summary>
  public string SecretKey { get; set; } = string.Empty;
}
