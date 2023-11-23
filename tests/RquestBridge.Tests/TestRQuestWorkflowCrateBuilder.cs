using ROCrates.Models;
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

  [Fact]
  public void AddAgent_Adds_AgentAsConfigured()
  {
    // Arrange
    var publishingOptions = new CratePublishingOptions();
    var workflowOptions = new WorkflowOptions();
    var organisationOptions = new CrateOrganizationOptions()
    {
      Id = Guid.NewGuid().ToString()
    };
    var projectOptions = new CrateProjectOptions()
    {
      Id = $"#project-{Guid.NewGuid()}"
    };
    var agentOptions = new CrateAgentOptions()
    {
      Id = Guid.NewGuid().ToString(),
      Affiliation = new Part() { Id = organisationOptions.Id },
      MemberOf = new List<Part> { new() { Id = projectOptions.Id } },
      Name = "Alice Day"
    };
    var builder = new RQuestWorkflowCrateBuilder(workflowOptions, publishingOptions, agentOptions, projectOptions,
      organisationOptions);

    // Act
    builder.AddAgent();
    var crate = builder.GetROCrate();
    crate.Entities.TryGetValue(agentOptions.Id, out var agentEntity);

    // Assert
    Assert.NotNull(agentEntity);
    Assert.Equal(agentOptions.Id, agentEntity.Id);

    var memberOf = agentEntity.GetProperty<List<Part>>("memberOf");
    Assert.NotNull(memberOf);
    Assert.Equal(agentOptions.MemberOf[0].Id, memberOf[0].Id);

    var affiliation = agentEntity.GetProperty<Part>("affiliation");
    Assert.NotNull(affiliation);
    Assert.Equal(agentOptions.Affiliation.Id, affiliation.Id);
  }

  [Fact]
  public void AddLicense_Adds_LicenseAsConfigured()
  {
    // Arrange
    var publishingOptions = new CratePublishingOptions
    {
      License = new LicenseOptions
      {
        Uri = "http://spdx.org/licenses/CC-BY-4.0",
        Properties = new LicenseProperties
        {
          Identifier = "CC-BY-4.0",
          Name = "Creative Commons Attribution 4.0 International"
        }
      }
    };
    var workflowOptions = new WorkflowOptions();
    var organisationOptions = new CrateOrganizationOptions();
    var projectOptions = new CrateProjectOptions();
    var agentOptions = new CrateAgentOptions();
    var builder = new RQuestWorkflowCrateBuilder(workflowOptions, publishingOptions, agentOptions, projectOptions,
      organisationOptions);

    // Act
    builder.AddLicense();
    var crate = builder.GetROCrate();
    crate.Entities.TryGetValue(publishingOptions.License.Uri, out var license);

    // Assert
    Assert.NotNull(license);
    Assert.Equal(publishingOptions.License.Uri, license.Id);
    Assert.Equal(publishingOptions.License.Properties.Identifier, license.GetProperty<string>("identifier"));
    Assert.Equal(publishingOptions.License.Properties.Name, license.GetProperty<string>("name"));
  }
}
