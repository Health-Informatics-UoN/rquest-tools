using Hutch.Rackit;
using Hutch.Rackit.TaskApi;
using Hutch.Relay.Config;
using Hutch.Relay.Constants;
using Hutch.Relay.Data;
using Hutch.Relay.Services;
using Hutch.Relay.Services.Contracts;
using Hutch.Relay.Services.Hosted;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hutch.Relay.Startup.Web;

public static class ConfigureWebService
{
  public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
  {
    var connectionString = builder.Configuration.GetConnectionString("Default");
    builder.Services.AddDbContext<ApplicationDbContext>(o => { o.UseNpgsql(connectionString); });

    builder.Services.AddIdentityCore<IdentityUser>(DefaultIdentityOptions.Configure)
      .AddEntityFrameworkStores<ApplicationDbContext>();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Upstream Task API
    builder.Services
      .Configure<ApiClientOptions>(builder.Configuration.GetSection("UpstreamTaskApi"))
      .AddHttpClient()
      .AddTransient<TaskApiClient>()
      .AddScoped<UpstreamTaskPoller>();

    // Task Queue
    builder.Services
      .Configure<RelayTaskQueueOptions>(builder.Configuration.GetSection("RelayTaskQueue"))
      .AddTransient<IRelayTaskQueue, RabbitRelayTaskQueue>(); // TODO: Azure / Other native queues

    // Other App Services
    builder.Services
      .AddTransient<RelayTaskService>()
      .AddTransient<RelaySubTaskService>()
      .AddTransient<SubNodeService>();

    // Hosted Services
    builder.Services.AddHostedService<BackgroundUpstreamTaskPoller>();

    return builder;
  }
}
