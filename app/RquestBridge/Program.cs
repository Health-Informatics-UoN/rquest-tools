using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RquestBridge.Config;
using RquestBridge.Models;
using RquestBridge.Services;
using RquestBridge.Services.Hosted;

namespace RquestBridge;

class Program
{
  public static void Main(string[] args)
  {
    IHost host = Host.CreateDefaultBuilder(args)
      .ConfigureServices((hostContext, services) =>
      {
        services.AddHttpClient<RQuestTaskApiClient>();
        services.AddScoped<RQuestAvailabilityPollingService>();
        services.AddHostedService<RQuestPollingHostedService>();
        services.AddTransient<RabbitJobQueueService>();
        services.AddOptions<RQuestOptions>().Bind(hostContext.Configuration.GetSection("RQuest"));
        services.AddOptions<RQuestTaskApiOptions>().Bind(hostContext.Configuration.GetSection("Credentials"));
      })
      .Build();
    host.Run();
  }}
