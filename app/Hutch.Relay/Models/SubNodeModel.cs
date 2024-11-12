using System.Collections.ObjectModel;

namespace Hutch.Relay.Models;

public class SubNodeModel
{
  public Guid Id { get; set; } = Guid.NewGuid();

  public ICollection<RelayUserModel> RelayUsers { get; set; } = new Collection<RelayUserModel>();
}
