namespace RquestBridge.Utilities;

public class RQuestWorkflowCrateDirector
{
  private readonly RQuestWorkflowCrateBuilder _builder;

  public RQuestWorkflowCrateDirector(RQuestWorkflowCrateBuilder builder)
  {
    _builder = builder;
  }

  public void BuildRQuestWorkflowCrate(string queryFileName, bool isAvailability, string dbHost, string dbName, string dbUser, string dbPassword)
  {
    _builder.AddMainEntity();
    _builder.AddProfile();
    _builder.AddLicense();
    _builder.AddCreateAction(queryFileName, isAvailability, dbHost, dbName, dbUser, dbPassword);
  }
}
