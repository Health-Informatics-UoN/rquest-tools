namespace RquestBridge.Config;

public class RQuestPollingOptions
{
  /// <summary>
  /// Polling interval in seconds
  /// for fetching jobs from Activity Sources
  /// </summary>
  public int PollingInterval { get; set; } = 5; 
}
