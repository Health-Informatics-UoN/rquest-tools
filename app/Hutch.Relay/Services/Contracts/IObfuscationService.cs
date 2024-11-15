using Hutch.Relay.Models;

namespace Hutch.Relay.Services.Contracts;

public interface IObfuscationService
{

  // /// <summary>
  // /// Get a RelayTask by id
  // /// </summary>
  // /// <param name="count">id of the RelayTask to get</param>
  // /// <returns>The RelayTaskModel of the id</returns>
  // /// <exception cref="KeyNotFoundException">The RelayTask does not exist.</exception>
  int LowNumberSuppression(int value, int threshold);

  // /// <summary>
  // /// Create a new RelayTask
  // /// </summary>
  // /// <param name="model">Model to Create.</param>
  // /// <returns>The newly created RelayTask.</returns>
  int Rounding(int value, int nearest);
}
