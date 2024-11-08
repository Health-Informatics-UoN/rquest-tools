namespace Hutch.Relay.Data.Entities;

public class SubTask
{
  public string Id { get; set; } = string.Empty;
  public SubNode Owner { get; set; } = new();
  public Task Task { get; set; } = new();
  public string? Result { get; set; }
}
