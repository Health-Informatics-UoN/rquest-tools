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

public class RabbitRelayTaskQueue : IRelayTaskQueue
{
  private readonly ConnectionFactory _factory;

  public RabbitRelayTaskQueue(IOptions<RelayTaskQueueOptions> options)
  {
    _factory = new()
    {
      Uri = new(options.Value.ConnectionString)
    };
  }

  private async Task<IChannel> ConnectChannel(string queueName)
  {
    await using var connection = await _factory.CreateConnectionAsync();
    var channel = await connection.CreateChannelAsync();

    await channel.QueueDeclareAsync(
      queue: queueName,
      durable: false,
      exclusive: false,
      autoDelete: false,
      arguments: null);

    return channel;
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
}
