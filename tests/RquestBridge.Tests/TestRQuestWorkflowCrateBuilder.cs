using ROCrates.Models;
using RquestBridge.Config;
using RquestBridge.Constants;
using RquestBridge.Utilities;

namespace RquestBridge.Tests;

public class TestRQuestWorkflowCrateBuilder
{
  [Fact]
  public void AddAgent_Adds_AgentAsConfigured()
  {
    // Arrange
    var publishingOptions = new CratePublishingOptions();
    var workflowOptions = new WorkflowOptions
    {
      Id = 471,
      Version = 3
    };
    var organisationOptions = new CrateOrganizationOptions()
    {
      Id = Guid.NewGuid().ToString()
    };
    var projectOptions = new CrateProjectOptions();
    var agentOptions = new CrateAgentOptions()
    {
      Id = Guid.NewGuid().ToString(),
      Affiliation = new Part() { Id = organisationOptions.Id },
      MemberOf = new List<Part> { new() { Id = projectOptions.Id } },
      Name = "Alice Day"
    };
    var agreementOptions = new AgreementPolicyOptions();
    var builder = new RQuestWorkflowCrateBuilder(workflowOptions, publishingOptions, agentOptions, projectOptions,
      organisationOptions, agreementOptions);

    // Act
    builder.AddAgent();
    var crate = builder.GetROCrate();
    crate.Entities.TryGetValue(agentOptions.Id, out var agentEntity);

    // Assert
    Assert.NotNull(agentEntity);
    Assert.Equal(agentOptions.Id, agentEntity.Id);

    var memberOf = agentEntity.GetProperty<List<Part>>("memberOf")!.Select(x => x.Id).ToList();
    Assert.NotNull(memberOf);
    var projectIds = crate.Entities.Keys.Where(x => x.StartsWith("#project-")).Select(x => new Part { Id = x })
      .ToList();
    foreach (var id in projectIds)
    {
      Assert.Contains(id.Id, memberOf);
    }

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
    var workflowOptions = new WorkflowOptions
    {
      Id = 471,
      Version = 3
    };
    var organisationOptions = new CrateOrganizationOptions();
    var projectOptions = new CrateProjectOptions();
    var agentOptions = new CrateAgentOptions();
    var agreementOptions = new AgreementPolicyOptions();
    var builder = new RQuestWorkflowCrateBuilder(workflowOptions, publishingOptions, agentOptions, projectOptions,
      organisationOptions, agreementOptions);

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
  public void AddAgent_Adds_ProjectAsConfigured()
  {
    // Arrange
    var publishingOptions = new CratePublishingOptions();
    var workflowOptions = new WorkflowOptions
    {
      Id = 471,
      Version = 3
    };
    var organisationOptions = new CrateOrganizationOptions();
    var projectOptions = new CrateProjectOptions
    {
      Name = "trefx"
    };
    var agentOptions = new CrateAgentOptions();
    var agreementOptions = new AgreementPolicyOptions();
    var builder = new RQuestWorkflowCrateBuilder(workflowOptions, publishingOptions, agentOptions, projectOptions,
      organisationOptions, agreementOptions);

    // Act
    builder.AddAgent();
    var crate = builder.GetROCrate();
    var projectId = crate.Entities.Keys.First(x => x.StartsWith("#project-"));
    crate.Entities.TryGetValue(projectId, out var project);


    // Assert
    Assert.NotNull(project);
    Assert.Equal(projectOptions.Name, project.GetProperty<string>("name"));
    Assert.Equal(projectOptions.Type, project.GetProperty<string>("@type"));
  }

  [Fact]
  public void AddCreateAction_Adds_AvailabilityCreateActionAsConfigured()
  {
    // Arrange
    var publishingOptions = new CratePublishingOptions();
    var workflowOptions = new WorkflowOptions
    {
      Name = "test-body",
      Id = 471,
      Version = 3
    };
    var organisationOptions = new CrateOrganizationOptions();
    var projectOptions = new CrateProjectOptions();
    var agentOptions = new CrateAgentOptions();
    var agreementOptions = new AgreementPolicyOptions();
    var builder = new RQuestWorkflowCrateBuilder(workflowOptions, publishingOptions, agentOptions, projectOptions,
      organisationOptions, agreementOptions);

    // Act
    builder.AddCreateAction(RquestQuery.FileName, true);
    var crate = builder.GetROCrate();
    var createActionId = crate.Entities.Keys.First(x => x.StartsWith("#query-"));
    crate.Entities.TryGetValue(createActionId, out var createAction);

    //Assert
    Assert.NotNull(createAction);
    var objectProperty = createAction.GetProperty<List<Part>>("object");
    Assert.NotNull(objectProperty);
    var ids = objectProperty.Select(x => x.Id).ToList();
    Assert.Contains(RquestQuery.FileName, ids);
    Assert.Contains($"#input_is_availability", ids);
  }

  [Fact]
  public void AddCreateAction_Adds_DistributionCreateActionAsConfigured()
  {
    // Arrange
    var publishingOptions = new CratePublishingOptions();
    var workflowOptions = new WorkflowOptions
    {
      Name = "test-body",
      Id = 471,
      Version = 3
    };
    var organisationOptions = new CrateOrganizationOptions();
    var projectOptions = new CrateProjectOptions();
    var agentOptions = new CrateAgentOptions();
    var agreementOptions = new AgreementPolicyOptions();
    var builder = new RQuestWorkflowCrateBuilder(workflowOptions, publishingOptions, agentOptions, projectOptions,
      organisationOptions, agreementOptions);

    // Act
    builder.AddCreateAction(RquestQuery.FileName, false);
    var crate = builder.GetROCrate();
    var createActionId = crate.Entities.Keys.First(x => x.StartsWith("#query-"));
    crate.Entities.TryGetValue(createActionId, out var createAction);

    //Assert
    Assert.NotNull(createAction);
    var objectProperty = createAction.GetProperty<List<Part>>("object");
    Assert.NotNull(objectProperty);
    var ids = objectProperty.Select(x => x.Id).ToList();
    Assert.Contains(RquestQuery.FileName, ids);
    Assert.Contains("#input_is_distribution", ids);
  }

  [Fact]
  public void AddSignOff_Adds_AssessActionAsConfigured()
  {
    // Arrange
    var publishingOptions = new CratePublishingOptions();
    var workflowOptions = new WorkflowOptions
    {
      Name = "test-body",
      Id = 471,
      Version = 3
    };
    var organisationOptions = new CrateOrganizationOptions();
    var projectOptions = new CrateProjectOptions();
    var agentOptions = new CrateAgentOptions();
    var agreementOptions = new AgreementPolicyOptions
      { Id = "https://agreement.example.org/agreement", Name = "Test Agreement Policy" };
    var builder = new RQuestWorkflowCrateBuilder(workflowOptions, publishingOptions, agentOptions, projectOptions,
      organisationOptions, agreementOptions);

    // Act
    builder.AddSignOff();
    var crate = builder.GetROCrate();
    var signOffActionId = crate.Entities.Keys.First(x => x.StartsWith("#signoff-"));
    crate.Entities.TryGetValue(signOffActionId, out var signOffAction);

    //Assert
    Assert.NotNull(signOffAction);
    var objectProperty = signOffAction.GetProperty<List<Part>>("object");
    Assert.NotNull(objectProperty);
    var ids = objectProperty.Select(x => x.Id).ToList();
    Assert.Contains("./", ids);
    var instrument = signOffAction.GetProperty<Part>("instrument");
    Assert.NotNull(instrument);
    Assert.Equal(agreementOptions.Id, instrument.Id);
  }
}
