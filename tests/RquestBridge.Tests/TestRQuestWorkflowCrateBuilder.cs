using RquestBridge.Config;
using RquestBridge.Utilities;

namespace RquestBridge.Tests;

public class TestRQuestWorkflowCrateBuilder
{
  [Fact]
  public void AddProfile_Adds_ProfileAsConfigured()
  {
    // Arrange
    var workflowOptions = new WorkflowOptions();
    var publishingOptions = new CratePublishingOptions();
    var organisationOptions = new CrateOrganizationOptions();
    var projectOptions = new CrateProjectOptions();
    var agentOptions = new CrateAgentOptions();
    var builder = new RQuestWorkflowCrateBuilder(workflowOptions, publishingOptions, agentOptions, projectOptions,
      organisationOptions);

    // Act
    builder.AddProfile();
    var crate = builder.GetROCrate();
    crate.Entities.TryGetValue("https://w3id.org/trusted-wfrun-crate/0.3", out var entity);

    // Assert
    Assert.NotNull(entity);
    Assert.Equal("https://w3id.org/trusted-wfrun-crate/0.3", entity.Id);
    Assert.Equal("Profile", entity.GetProperty<string>("@type"));
  }
}
