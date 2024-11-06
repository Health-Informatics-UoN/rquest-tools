using Hutch.Rackit;
using Microsoft.Extensions.Options;
using RackitUsage;


// Choose a Sample behaviour to run
var validArg = Enum.TryParse<Samples>(args[1], ignoreCase: true, out var sample);
if (!validArg) sample = Samples.SimpleCheck;

// If in a polling mode, how long do we run the app for?
var pollFor = TimeSpan.FromMinutes(1);

// Configuration
var options = Options.Create(new ApiClientOptions
{
  // Fill in your connection details
});


// Run the desired sample pathway
switch(sample)
{
  case Samples.SimpleCheck:
    await new Simple(options).PollAvailability(pollFor);
    break;
  case Samples.SimplePolling:
    await new Simple(options).CheckAllQueuesOnce();
    break;
  case Samples.GenericHostCheck:
    break;
  case Samples.GenericHostPolling:
    break;
  default:
    throw new ArgumentOutOfRangeException();
};

internal enum Samples
{
  SimpleCheck,
  SimplePolling,
  GenericHostCheck,
  GenericHostPolling
}
