using System.CommandLine;
using Hutch.Relay.Commands.Helpers;
using Hutch.Relay.Constants;
using Hutch.Relay.Data;
using Hutch.Relay.Data.Entities;
using Hutch.Relay.Services;
using Microsoft.EntityFrameworkCore;

namespace Hutch.Relay.Commands;

internal class AddUser : Command
{
  public AddUser(string name)
    : base(name, "Add a new User.")
  {
    var argUserName = new Argument<string>("username", "The new user name.");
    Add(argUserName);
    
    this.SetHandler(
      async (
        logger, config, console,
        username) =>
      {
        // figure out the connection string from the option, or config
        var connectionString = config.GetConnectionString("RelayDb");

        // Configure DI and run the command handler
        await this
          .ConfigureServices(s =>
          {
            ServiceCollectionServiceExtensions.AddSingleton<ILoggerFactory>(s, _ => logger)
              .AddSingleton<IConfiguration>(_ => config)
              .AddSingleton<IConsole>(_ => console)
              .AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(connectionString));
            LoggingServiceCollectionExtensions.AddLogging(s)
              .AddIdentityCore<RelayUser>(DefaultIdentityOptions.Configure)
              .AddEntityFrameworkStores<ApplicationDbContext>();
            ServiceCollectionServiceExtensions.AddTransient<SubNodeService>(s);
            ServiceCollectionServiceExtensions.AddTransient<Runners.AddUser>(s);
          })
          .GetRequiredService<Runners.AddUser>()
          .Run(username);
      },
      Bind.FromServiceProvider<ILoggerFactory>(),
      Bind.FromServiceProvider<IConfiguration>(),
      Bind.FromServiceProvider<IConsole>(),
      argUserName);
  }
}
