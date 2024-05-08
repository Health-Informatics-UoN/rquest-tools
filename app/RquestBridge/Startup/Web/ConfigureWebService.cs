using Microsoft.FeatureManagement;
using RquestBridge.Config;
using RquestBridge.Services;
using RquestBridge.Services.Hosted;

namespace RquestBridge.Startup.Web;

public static class ConfigureWebService
{
  public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder b)
  {
    b.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    b.Services.AddEndpointsApiExplorer();
    b.Services.AddSwaggerGen();
    b.Services.AddFeatureManagement(
      b.Configuration.GetSection("Flags"));

    // Add Options
    b.Services
      .Configure<RQuestOptions>(b.Configuration.GetSection("RQuest"))
      .Configure<RQuestTaskApiOptions>(b.Configuration.GetSection("Credentials"))
      .Configure<WorkflowOptions>(b.Configuration.GetSection("Workflow"))
      .Configure<CrateAgentOptions>(b.Configuration.GetSection("Crate:Agent"))
      .Configure<CrateProjectOptions>(b.Configuration.GetSection("Crate:Project"))
      .Configure<CrateOrganizationOptions>(b.Configuration.GetSection("Crate:Organisation"))
      .Configure<BridgeOptions>(b.Configuration.GetSection("Bridge"))
      .Configure<MinioOptions>(MinioOptions.System, b.Configuration.GetSection("Minio:System"))
      .Configure<MinioOptions>(MinioOptions.External, b.Configuration.GetSection("Minio:External"))
      .Configure<HutchDatabaseConnectionDetails>(b.Configuration.GetSection("HutchAgent:DBConnection"))
      .Configure<HutchAgentOptions>(b.Configuration.GetSection("HutchAgent:API"))
      .Configure<AssessActionsOptions>(b.Configuration.GetSection("AssessActions"))
      .Configure<AgreementPolicyOptions>(b.Configuration.GetSection("AgreementPolicy"));

    // Add HttpClients
    b.Services.AddHttpClient<RQuestTaskApiClient>();
    b.Services.AddHttpClient<HutchApiClient>();

    // Add Services
    b.Services
      .AddScoped<RQuestAvailabilityPollingService>()
      .AddHostedService<RQuestPollingHostedService>()
      .AddTransient<CrateGenerationService>()
      .AddTransient<MinioService>()
      .AddTransient<ResultsHandlingService>();

    return b;
  }
}
