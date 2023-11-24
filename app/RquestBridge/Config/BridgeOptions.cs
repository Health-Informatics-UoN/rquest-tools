namespace RquestBridge.Config;

public class BridgeOptions
{
  public string WorkingDirectoryBase { get; set; } = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "bridge-workdir");
}
