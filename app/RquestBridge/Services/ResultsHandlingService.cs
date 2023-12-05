using RquestBridge.Config;
using RquestBridge.Models;

namespace RquestBridge.Services;

public class ResultsHandlingService
{
  private readonly RQuestTaskApiOptions _taskApiOptions;

  public ResultsHandlingService(RQuestTaskApiOptions taskApiOptions)
  {
    _taskApiOptions = taskApiOptions;
  }

  public async Task HandleResults(FinalOutcomeWebHookModel payload)
  {
  }
}
