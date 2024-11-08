using System.Collections.ObjectModel;

namespace Hutch.Relay.Data.Entities;

public class SubNode
{
  public string Id { get; set; } = string.Empty;

  public ICollection<RelayUser> RelayUsers { get; set; } = new Collection<RelayUser>();
}
