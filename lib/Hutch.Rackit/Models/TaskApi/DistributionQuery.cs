using System.Text.Json.Serialization;

namespace Hutch.Rackit.Models.TaskApi;

public class DistributionQuery : TaskApiBaseResponse
{
  /// <summary>
  /// The code for the type of distribution query.
  /// Possible values: "GENERIC", "DEMOGRAPHICS" or "ICD-MAN"
  /// </summary>
  [JsonPropertyName("code")]
  public string Code { get; set; } = string.Empty;

  /// <summary>
  /// The type of analysis to be carried out.
  /// Possible values: "DISTRIBUTION"
  /// </summary>
  [JsonPropertyName("analysis")]
  public string Analysis { get; set; } = "DISTRIBUTION";
}
