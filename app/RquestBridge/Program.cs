using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using RquestBridge.Commands.Helpers;
using RquestBridge.Startup;
using RquestBridge.Startup.Cli;

await new CommandLineBuilder(new CliEntrypoint())
  .UseDefaults()
  .UseRootCommandBypass(args, WebEntrypoint.Run)
  .UseCliHostDefaults(args)
  .Build()
  .InvokeAsync(args);
