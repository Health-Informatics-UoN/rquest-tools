using System.Text.Json.Serialization;

namespace RquestBridge.Dto.RQuestResults;

/// <summary>
/// This class represents the overall result of an RQuest job.
/// </summary>
public class RquestQueryResult
{
  [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;

  [JsonPropertyName("protocolVersion")] public string ProtocolVersion { get; set; } = "v2";

  [JsonPropertyName("collection_id")] public string CollectionId { get; set; } = string.Empty;

  [JsonPropertyName("uuid")] public string Uuid { get; set; } = string.Empty;

  [JsonPropertyName("queryResult")] public QueryResult Results { get; set; } = new();

  [JsonPropertyName("message")] public string? Message { get; set; } = null;
}
