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
}
