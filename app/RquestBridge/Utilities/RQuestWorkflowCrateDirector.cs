namespace RquestBridge.Utilities;

public class RQuestWorkflowCrateDirector
{
  private readonly RQuestWorkflowCrateBuilder _builder;

  public RQuestWorkflowCrateDirector(RQuestWorkflowCrateBuilder builder)
  {
    _builder = builder;
  }

  /// <summary>
  /// Build the fully configured workflow crate.
  /// </summary>
  /// <param name="queryFileName">The name of the file to save the query.</param>
  /// <param name="isAvailability">Is the the query an availability query.</param>
  public void BuildRQuestWorkflowCrate(string queryFileName, bool isAvailability)
  {
    _builder.AddMainEntity();
    _builder.AddProfile();
    _builder.AddLicense();
    _builder.AddCreateAction(queryFileName, isAvailability);
    _builder.AddAgent();
  }
}
