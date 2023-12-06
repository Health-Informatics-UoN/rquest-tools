using System.Text.Json.Serialization;

namespace RquestBridge.Dto.RQuestResults;

/// <summary>
/// This class represents the <c>queryResult</c> field in an <c>RquestQueryResult</c> object.
/// <seealso cref="RquestQueryResult"/>
/// </summary>
public class QueryResult
{
  [JsonPropertyName("count")] public int Count { get; set; } = 0;

  [JsonPropertyName("datasetCount")] public int DatasetCount { get; set; } = 0;

  [JsonPropertyName("files")] public List<RquestFile> Files { get; set; } = new();
}
