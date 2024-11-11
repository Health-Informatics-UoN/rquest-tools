using Hutch.Relay.Data;
using Hutch.Relay.Data.Entities;
using Hutch.Relay.Models;

namespace Hutch.Relay.Services;

public class RelaySubTaskService(ApplicationDbContext db)
{
  /// <summary>
  /// Create a new RelaySubTask
  /// </summary>
  /// <returns>The newly created RelaySubTask</returns>
  public async Task<RelaySubTaskModel> Create(string relayTaskId, string ownerId)
  {
    // Not 100% convinced by the multiple db reads here to satisfy nice error messages?
    var subNode = await db.SubNodes.FindAsync(ownerId) ??
                  throw new KeyNotFoundException($"The specified owner does not exist: {ownerId}");
    var parent = await db.RelayTasks.FindAsync(relayTaskId) ??
                 throw new KeyNotFoundException($"The specified parent Relay Task does not exist: {relayTaskId}");

    var entity = new RelaySubTask
    {
      Id = $"{relayTaskId}_{subNode.Id}",
      Owner = subNode,
      RelayTask = parent
    };

    db.RelaySubTasks.Add(entity);
    await db.SaveChangesAsync();

    // TODO: what's actually useful here?
    return new()
    {
      Id = entity.Id,
      Owner = new()
      {
        Id = subNode.Id
      },
      RelayTask = new() // TODO: Automapper or something more sane than this?
      {
        Id = parent.Id,
        Collection = parent.Collection,
        CreatedAt = parent.CreatedAt,
        CompletedAt = parent.CompletedAt,
      }
    };
  }

  /// <summary>
  /// Set the Result of a RelaySubTask
  /// </summary>
  /// <param name="id">id of the RelaySubTask</param>
  /// <param name="result">Result value to set</param>
  /// <returns>The updated RelaySubTask</returns>
  /// <exception cref="KeyNotFoundException"></exception>
  public async Task<RelaySubTaskModel> SetResult(string id, string result)
  {
    var entity = await db.RelaySubTasks.FindAsync(id)
                 ?? throw new KeyNotFoundException();

    entity.Result = result;
    db.RelaySubTasks.Update(entity);
    await db.SaveChangesAsync();

    return new RelaySubTaskModel
    {
      Id = entity.Id,
      Owner = new SubNodeModel
      {
        Id = entity.Owner.Id
      },
      Result = entity.Result
    };
  }
}
