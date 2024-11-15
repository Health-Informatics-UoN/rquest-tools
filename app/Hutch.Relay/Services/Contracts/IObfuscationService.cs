using Hutch.Relay.Models;

namespace Hutch.Relay.Services.Contracts;

public interface IObfuscationService
{

  /// <summary>
  /// Get a RelayTask by id
  /// </summary>
  /// <param name="id">id of the RelayTask to get</param>
  /// <returns>The RelayTaskModel of the id</returns>
  /// <exception cref="KeyNotFoundException">The RelayTask does not exist.</exception>
  Task<ObfuscationModel> LowNumberSuppression(string id);

  /// <summary>
  /// Create a new RelayTask
  /// </summary>
  /// <param name="model">Model to Create.</param>
  /// <returns>The newly created RelayTask.</returns>
  Task<ObfuscationModel> Rounding(int model);
}
