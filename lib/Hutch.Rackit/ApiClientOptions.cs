using System.Text;

namespace Hutch.Rackit;

/// <summary>
/// Options model for the API Client Services
/// </summary>
public class ApiClientOptions
{
  /// <summary>
  /// Base Url of the API to interact with
  /// </summary>
  public required string BaseUrl { get; set; }

  /// <summary>
  /// The Collection ID to use with API interactions
  /// </summary>
  public required string CollectionId { get; set; }

  /// <summary>
  /// Username to access the API. Passed in Basic Auth header.
  /// </summary>
  public required string Username { get; set; }

  /// <summary>
  /// Password to access the API. Passed in Basic Auth header.
  /// </summary>
  public required string Password { get; set; }

  /// <summary>
  /// <see cref="Username"/> and <see cref="Password"/> correctly Base64 encoded for use in a Basic Auth header
  /// </summary>
  public string BasicCredentials => Convert.ToBase64String(
    Encoding.UTF8.GetBytes($"{Username}:${Password}"));
}
