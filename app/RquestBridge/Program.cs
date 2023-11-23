using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RquestBridge.Config;
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
        services.AddTransient<CrateGenerationService>();
        services.AddOptions<RQuestOptions>().Bind(hostContext.Configuration.GetSection("RQuest"));
        services.AddOptions<RQuestTaskApiOptions>().Bind(hostContext.Configuration.GetSection("Credentials"));
        services.AddOptions<WorkflowOptions>().Bind(hostContext.Configuration.GetSection("Workflow"));
        services.AddOptions<CrateAgentOptions>().Bind(hostContext.Configuration.GetSection("Crate:Agent"));
        services.AddOptions<CrateProjectOptions>().Bind(hostContext.Configuration.GetSection("Crate:Project"));
        services.AddOptions<CrateOrganizationOptions>().Bind(hostContext.Configuration.GetSection("Crate:Organisation"));
      })
      .Build();
    host.Run();
  }}
