using Flurl;
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
    var profileOptions = new CrateProfileOptions();
    var builder = new RQuestWorkflowCrateBuilder(workflowOptions, publishingOptions, agentOptions, projectOptions,
      organisationOptions, profileOptions);

    // Act
    builder.AddProfile();
    var crate = builder.GetROCrate();
    crate.Entities.TryGetValue(profileOptions.Id, out var entity);

    // Assert
    Assert.NotNull(entity);
    Assert.Equal(profileOptions.Id, entity.Id);
    Assert.Equal(profileOptions.Type, entity.GetProperty<string>("@type"));
    Assert.Equal(profileOptions.Name, entity.GetProperty<string>("name"));
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
    var profileOptions = new CrateProfileOptions();
    var builder = new RQuestWorkflowCrateBuilder(workflowOptions, publishingOptions, agentOptions, projectOptions,
      organisationOptions, profileOptions);

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
    var profileOptions = new CrateProfileOptions();
    var builder = new RQuestWorkflowCrateBuilder(workflowOptions, publishingOptions, agentOptions, projectOptions,
      organisationOptions, profileOptions);

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

  [Fact]
  public void AddMainEntity_Adds_MainEntityAsConfigured()
  {
    // Arrange
    var publishingOptions = new CratePublishingOptions();
    var workflowOptions = new WorkflowOptions
    {
      Id = 471,
      Version = 3
    };
    var organisationOptions = new CrateOrganizationOptions();
    var projectOptions = new CrateProjectOptions();
    var agentOptions = new CrateAgentOptions();
    var profileOptions = new CrateProfileOptions();
    var builder = new RQuestWorkflowCrateBuilder(workflowOptions, publishingOptions, agentOptions, projectOptions,
      organisationOptions, profileOptions);

    var expectedId = Url.Combine(workflowOptions.BaseUrl, workflowOptions.Id.ToString())
      .SetQueryParam("version", workflowOptions.Version.ToString());
    var expectedMainEntityPart = new Part { Id = expectedId };

    // Act
    builder.AddMainEntity();
    var crate = builder.GetROCrate();
    crate.Entities.TryGetValue(expectedId, out var mainEntity);
    var actualMainEntityPart = crate.RootDataset.GetProperty<Part>("mainEntity");


    // Assert
    Assert.NotNull(mainEntity);
    Assert.Equal(expectedId, mainEntity.Id);
    Assert.NotNull(mainEntity.GetProperty<Part>("distribution"));
    // Should be null as `AddProfile` was not called
    Assert.Null(mainEntity.GetProperty<Part>("conformsTo"));
    Assert.NotNull(actualMainEntityPart);
    Assert.Equal(expectedMainEntityPart.Id, actualMainEntityPart.Id);
  }

  [Fact]
  public void AddProject_Adds_ProjectAsConfigured()
  {
    // Arrange
    var publishingOptions = new CratePublishingOptions();
    var workflowOptions = new WorkflowOptions();
    var organisationOptions = new CrateOrganizationOptions();
    var projectOptions = new CrateProjectOptions
    {
      Name = "trefx"
    };
    var agentOptions = new CrateAgentOptions();
    var profileOptions = new CrateProfileOptions();
    var builder = new RQuestWorkflowCrateBuilder(workflowOptions, publishingOptions, agentOptions, projectOptions,
      organisationOptions, profileOptions);

    // Act
    builder.AddProject();
    var crate = builder.GetROCrate();
    var projectId = crate.Entities.Keys.First(x => x.StartsWith("#project-"));
    crate.Entities.TryGetValue(projectId, out var project);


    // Assert
    Assert.NotNull(project);
    Assert.Equal(projectOptions.Name, project.GetProperty<string>("name"));
    Assert.Equal(projectOptions.Type, project.GetProperty<string>("@type"));
  }
}
