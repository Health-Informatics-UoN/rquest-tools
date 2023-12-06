using System.CommandLine;

namespace RquestBridge.Startup.Cli;

public class CliEntrypoint : RootCommand
{
  public CliEntrypoint() : base("RQuest to HutchAgent bridge")
  {
    AddGlobalOption(new Option<string>(new[] { "--environment", "-e" }));

    // Add Commands here
  }
}
