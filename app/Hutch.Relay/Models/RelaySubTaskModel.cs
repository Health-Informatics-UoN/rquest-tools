namespace Hutch.Relay.Models;

public class RelaySubTaskModel
{
  public string Id { get; set; } = string.Empty;
  public SubNodeModel Owner { get; set; } = new();
  public RelayTaskModel RelayTask { get; set; } = new();
  public string? Result { get; set; }
}
