using Hutch.Relay.Models;

namespace Hutch.Relay.Services.Contracts;

public interface IRelaySubTaskService
{
  /// <summary>
  /// Create a new RelaySubTask
  /// </summary>
  /// <returns>The newly created RelaySubTask</returns>
  Task<RelaySubTaskModel> Create(string relayTaskId, Guid ownerId);

  /// <summary>
  /// Set the Result of a RelaySubTask
  /// </summary>
  /// <param name="id">id of the RelaySubTask</param>
  /// <param name="result">Result value to set</param>
  /// <returns>The updated RelaySubTask</returns>
  /// <exception cref="KeyNotFoundException"></exception>
  Task<RelaySubTaskModel> SetResult(Guid id, string result);


  /// <summary>
  /// Get a SubTask by ID
  /// </summary>
  /// <param name="id">Subtask Id</param>
  /// <returns>RelaySubTaskModel from the ID</returns>
  /// <exception cref="KeyNotFoundException">The RelaySubTask does not exist.</exception>
  Task<RelaySubTaskModel> Get(Guid id);

  /// <summary>
  /// List RelaySubTasks that have not been completed for a given RelayTask.
  /// </summary>
  /// <returns>The list of incomplete RelaySubTasks</returns>
  Task<IEnumerable<RelaySubTaskModel>> ListIncomplete(string relayTaskId);

}
