using System.CommandLine;

namespace Hutch.Relay.Startup.Cli;

public class CliEntrypoint : RootCommand
{
  public CliEntrypoint() : base("Hutch Relay")
  {
    AddGlobalOption(new Option<string>(new[] { "--environment", "-e" }));

    // Add Commands here
  }
}
