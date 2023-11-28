namespace RquestBridge.Config;

public class WorkflowOptions
{
  public int Version { get; set; }
  
  public int Id { get; set; }

  public string BaseUrl { get; set; } = "https://workflowhub.eu/workflows";

  public string Name {get; set; } = "rquest-omop-worker-workflow";
}
