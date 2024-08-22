using System.Text.Json.Serialization;

namespace Hutch.Rackit.Models.TaskApi;

/// <summary>
/// This class represents an individual file in the list <c>QueryResult.Files</c>.
/// <seealso cref="QueryResult"/>
/// </summary>
public class ResultFile
{
  [JsonPropertyName("file_data")] public string FileData { get; set; } = string.Empty;

  [JsonPropertyName("file_name")] public string FileName { get; set; } = string.Empty;

  [JsonPropertyName("file_description")] public string? FileDescription { get; set; } = null;

  [JsonPropertyName("file_reference")] public string FileReference { get; set; } = string.Empty;

  [JsonPropertyName("file_sensitive")] public bool FileSensitive { get; set; } = true;

  [JsonPropertyName("file_size")] public double FileSize { get; set; } = 0.0;

  [JsonPropertyName("file_type")] public string FileType { get; set; } = "BCOS";
}
