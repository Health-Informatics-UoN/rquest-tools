using Hutch.Relay.Auth.Basic;
using Hutch.Relay.Constants;
using Hutch.Relay.Data;
using Hutch.Relay.Data.Entities;
using Hutch.Relay.Services;
using Microsoft.EntityFrameworkCore;

namespace Hutch.Relay.Startup.Web;

public static class ConfigureWebService
{
  public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
  {
    var connectionString = builder.Configuration.GetConnectionString("RelayDb");
    builder.Services.AddDbContext<ApplicationDbContext>(o => { o.UseNpgsql(connectionString); });
    
    builder.Services.AddIdentityCore<RelayUser>(DefaultIdentityOptions.Configure)
      .AddEntityFrameworkStores<ApplicationDbContext>();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    
    builder.Services.AddAuthentication("Basic")
      .AddScheme<BasicAuthSchemeOptions, BasicAuthHandler>("Basic", opts => 
      {
        opts.Realm = "relay";
      });

    // Add Options

    // Add Services
    builder.Services
      .AddTransient<RelayTaskService>()
      .AddTransient<RelaySubTaskService>()
      .AddTransient<SubNodeService>();
    return builder;
  }
}
