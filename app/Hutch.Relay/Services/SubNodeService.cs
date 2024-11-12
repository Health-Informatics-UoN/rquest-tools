using Hutch.Relay.Data;
using Hutch.Relay.Data.Entities;
using Hutch.Relay.Models;
using Microsoft.EntityFrameworkCore;

namespace Hutch.Relay.Services;

public class SubNodeService(ApplicationDbContext db)
{
  /// <summary>
  /// Create a new SubNode
  /// </summary>
  /// <param name="subNodeModel">SubNode model</param>
  /// <param name="relayUser">The relay user registering the SubNode</param>
  /// <returns>The SubNode created.</returns>
  public async Task<SubNodeModel> Create(SubNodeModel subNodeModel, RelayUser relayUser)
  {
    var entity = new SubNode()
    {
      Id = subNodeModel.Id
    };
    entity.RelayUsers.Add(relayUser);
    db.SubNodes.Add(entity);
    await db.SaveChangesAsync();

    var model = new SubNodeModel
    {
      Id = entity.Id,
      Owner = entity.RelayUsers.First().UserName ?? string.Empty
    };
    return model;
  }

  /// <summary>
  /// List all registered sub nodes
  /// </summary>
  /// <returns>A list of nodes</returns>
  public async Task<IEnumerable<SubNodeModel>> List()
  {
    var entities = await db.SubNodes.AsNoTracking()
      .Include(x => x.RelayUsers)
      .ToListAsync();

    return entities.Select(x => new SubNodeModel
    {
      Id = x.Id,
      Owner = x.RelayUsers.First().UserName ?? string.Empty
    });
  }
}
