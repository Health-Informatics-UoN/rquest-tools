using Hutch.Relay.Models;

namespace Hutch.Relay.Services.Contracts;

public interface IRelayTaskService
{
  /// <summary>
  /// Get a RelayTask by id
  /// </summary>
  /// <param name="id">id of the RelayTask to get</param>
  /// <returns>The RelayTaskModel of the id</returns>
  /// <exception cref="KeyNotFoundException">The RelayTask does not exist.</exception>
  Task<RelayTaskModel> Get(string id);

  /// <summary>
  /// Create a new RelayTask
  /// </summary>
  /// <param name="model">Model to Create.</param>
  /// <returns>The newly created RelayTask.</returns>
  Task<RelayTaskModel> Create(RelayTaskModel model);

  /// <summary>
  /// Set a RelayTask as complete.
  /// </summary>
  /// <param name="id">The id of the task to complete.</param>
  /// <returns></returns>
  /// <exception cref="KeyNotFoundException">The RelayTask does not exist.</exception>
  Task<RelayTaskModel> SetComplete(string id);

  /// <summary>
  /// List RelayTasks that have not been completed.
  /// </summary>
  /// <returns>The list of incomplete RelayTasks</returns>
  Task<IEnumerable<RelayTaskModel>> ListIncomplete();
}
