using System.Collections.ObjectModel;

namespace Hutch.Relay.Models;

public class SubNodeModel
{
  public string Id { get; set; } = Guid.NewGuid().ToString();

  public ICollection<RelayUserModel> RelayUsers { get; set; } = new Collection<RelayUserModel>();
}
