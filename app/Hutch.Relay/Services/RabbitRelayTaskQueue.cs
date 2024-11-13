using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Hutch.Rackit.TaskApi.Models;
using Hutch.Relay.Config;
using Hutch.Relay.Models;
using Hutch.Relay.Services.Contracts;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Hutch.Relay.Services;

public class RabbitRelayTaskQueue(
  ILogger<RabbitRelayTaskQueue> logger,
  IOptions<RelayTaskQueueOptions> options)
  : IRelayTaskQueue, IDisposable, IAsyncDisposable
{
  private readonly ConnectionFactory _factory = new()
  {
    Uri = new(options.Value.ConnectionString)
  };

  private IConnection? _connection;

  private async Task<IChannel> ConnectChannel(string queueName)
  {
    if (_connection is not null && !_connection.IsOpen)
    {
      await _connection.DisposeAsync();
      _connection = null;
    }

    _connection ??= await _factory.CreateConnectionAsync();

    var channel = await _connection.CreateChannelAsync();

    await channel.QueueDeclareAsync(
      queue: queueName,
      durable: false,
      exclusive: false,
      autoDelete: false,
      arguments: null);

    return channel;
  }

  public async Task<bool> IsReady(string? queueName = null)
  {
    try
    {
      await using var channel = await ConnectChannel(queueName ?? "readyTest");
    }
    catch (Exception e) // It's OK that this is broad; any exception while trying to do this means the app is unusable.
    {
      logger.LogCritical(e, "{ExceptionMessage}", e.Message);
      return false;
    }

    return true;
  }

  public async Task Send<T>(string subnodeId, T message) where T : TaskApiBaseResponse
  {
    await using var channel = await ConnectChannel(subnodeId);

    var body = Encoding.UTF8.GetBytes(
      JsonSerializer.Serialize(message));

    await channel.BasicPublishAsync(
      exchange: string.Empty,
      routingKey: subnodeId,
      body: body,
      mandatory: false,
      basicProperties: new BasicProperties
      {
        Type = typeof(T).Name
      });
  }

  public void Dispose()
  {
    _connection?.Dispose();
  }

  public async ValueTask DisposeAsync()
  {
    if (_connection is not null) await _connection.DisposeAsync();
  }
}
