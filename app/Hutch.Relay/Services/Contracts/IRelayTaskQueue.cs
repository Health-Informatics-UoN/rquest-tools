using Hutch.Rackit.TaskApi.Models;
using Hutch.Relay.Models;

namespace Hutch.Relay.Services.Contracts;

/// <summary>
/// An interface for the functional needs of a RelayTask Queue.
///
/// This is quite domain opinionated - not a general purpose queue service;
/// it cares about Relay SubNodes and Task Types etc.
/// </summary>
public interface IRelayTaskQueue
{
  /// <summary>
  /// Test if the configured queue backend is available to receive messages, optionally on a specific named queue
  /// </summary>
  /// <param name="queueName">Optionally specify a queue name to also ensure is ready</param>
  /// <returns>True if the queue service is ready to receive items (in the named queue if provided)</returns>
  public Task<bool> IsReady(string? queueName = null);
  
  /// <summary>
  /// Send a message with the provided RelayTask Body to the queue
  /// </summary>
  /// <param name="subnodeId">The ID for the SubNode this RelayTask is intended for</param>
  /// <param name="taskBody">The body of the task; may be any valid Task type</param>
  /// <typeparam name="T">The Task type for the provided body</typeparam>
  /// <returns></returns>
  public Task Send<T>(string subnodeId, T taskBody) where T : TaskApiBaseResponse;

  // TODO: Pop a message from the queue
  // Typing is much harder here. Generics is a bit rubbish?
  // Maybe use a Result wrapper?
  public Task<(Type type, TaskApiBaseResponse task)?> Pop(string subnodeId);
}
