using Hutch.Relay.Data;
using Hutch.Relay.Data.Entities;
using Hutch.Relay.Models;
using Hutch.Relay.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Hutch.Relay.Services;

public class RelaySubTaskService(ApplicationDbContext db) : IRelaySubTaskService
{
  /// <summary>
  /// Create a new RelaySubTask
  /// </summary>
  /// <returns>The newly created RelaySubTask</returns>
  public async Task<RelaySubTaskModel> Create(string relayTaskId, Guid ownerId)
  {
    // Not 100% convinced by the multiple db reads here to satisfy nice error messages?
    var subNode = await db.SubNodes
                    .Include(x => x.RelayUsers)
                    .SingleAsync(x => x.Id == ownerId)
                  ?? throw new KeyNotFoundException($"The specified owner does not exist: {ownerId}");

    var parent = await db.RelayTasks.FindAsync(relayTaskId)
                 ?? throw new KeyNotFoundException($"The specified parent Relay Task does not exist: {relayTaskId}");

    var entity = new RelaySubTask
    {
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
        Id = subNode.Id,
        Owner = subNode.RelayUsers.First().UserName ?? string.Empty
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
  public async Task<RelaySubTaskModel> SetResult(Guid id, string result)
  {
    var entity = await db.RelaySubTasks
                   .Include(x => x.Owner)
                   .ThenInclude(x => x.RelayUsers)
                   .Include(x => x.RelayTask)
                   .SingleAsync(x => x.Id == id)
                 ?? throw new KeyNotFoundException();

    entity.Result = result;
    db.RelaySubTasks.Update(entity);
    await db.SaveChangesAsync();

    return new()
    {
      Id = entity.Id,
      Owner = new()
      {
        Id = entity.Owner.Id,
        Owner = entity.Owner.RelayUsers.First()
                  .UserName ??
                string.Empty
      },
      Result = entity.Result,
      RelayTask = new()
      {
        Id = entity.RelayTask.Id,
        Collection = entity.RelayTask.Collection,
        CreatedAt = entity.RelayTask.CreatedAt,
        CompletedAt = entity.RelayTask.CompletedAt,
      }
    };
  }

  /// <summary>
  /// Get a SubTask by ID
  /// </summary>
  /// <param name="id">Subtask Id</param>
  /// <returns>RelaySubTaskModel from the ID</returns>
  /// <exception cref="KeyNotFoundException">The RelaySubTask does not exist.</exception>
  public async Task<RelaySubTaskModel> Get(Guid id)
  {
    var entity = await db.RelaySubTasks.AsNoTracking().Include(relaySubTask => relaySubTask.Owner)
                   .ThenInclude(subNode => subNode.RelayUsers)
                   .Include(relaySubTask => relaySubTask.RelayTask)
                   .SingleOrDefaultAsync(t => t.Id == id)
                 ?? throw new KeyNotFoundException();

    return new RelaySubTaskModel()
    {
      Id = entity.Id,
      Owner = new SubNodeModel
      {
        Id = entity.Owner.Id,
        Owner = entity.Owner.RelayUsers.First().UserName ?? string.Empty
      },
      RelayTask = new RelayTaskModel()
      {
        Id = entity.RelayTask.Id
      },
      Result = entity.Result
    };
  }

  /// <summary>
  /// List RelaySubTasks that have not been completed for a given RelayTask.
  /// </summary>
  /// <returns>The list of incomplete RelaySubTasks</returns>
  public async Task<IEnumerable<RelaySubTaskModel>> ListIncomplete(string relayTaskId)
  {
    var query = await db.RelaySubTasks
      .AsNoTracking()
      .Where(x => x.RelayTask.Id == relayTaskId && x.Result == null)
      .Include(relaySubTask => relaySubTask.Owner).ThenInclude(subNode => subNode.RelayUsers)
      .Include(relaySubTask => relaySubTask.RelayTask)
      .ToListAsync();

    return query.Select(x => new RelaySubTaskModel()
    {
      Id = x.Id,
      Owner = new SubNodeModel()
      {
        Id = x.Owner.Id,
        Owner = x.Owner.RelayUsers.First().UserName ?? string.Empty
      },
      RelayTask = new RelayTaskModel()
      {
        Id = x.RelayTask.Id
      }
    });
  }
}
