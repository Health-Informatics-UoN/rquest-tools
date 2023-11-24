using System.Globalization;
using System.Text.Json;
using Flurl;
using ROCrates;
using ROCrates.Models;
using RquestBridge.Config;
using RquestBridge.Constants;
using RquestBridge.Contracts;

namespace RquestBridge.Utilities;

public class RQuestWorkflowCrateBuilder : IROCrateBuilder
{
  private readonly CrateAgentOptions _crateAgentOptions;

  private readonly CrateOrganizationOptions _crateOrganizationOptions;
  private readonly CrateProfileOptions _crateProfileOptions;
  private readonly CrateProjectOptions _crateProjectOptions;
  private readonly CratePublishingOptions _publishingOptions;
  private readonly WorkflowOptions _workflowOptions;
  private ROCrate _crate = new ROCrate();

  public RQuestWorkflowCrateBuilder(WorkflowOptions workflowOptions, CratePublishingOptions publishingOptions,
    CrateAgentOptions crateAgentOptions, CrateProjectOptions crateProjectOptions,
    CrateOrganizationOptions crateOrganizationOptions, CrateProfileOptions crateProfileOptions)
  {
    _workflowOptions = workflowOptions;
    _publishingOptions = publishingOptions;
    _crateAgentOptions = crateAgentOptions;
    _crateProjectOptions = crateProjectOptions;
    _crateOrganizationOptions = crateOrganizationOptions;
    _crateProfileOptions = crateProfileOptions;

    // Add 5 Safes props to RootDataset
    UpdateRootDataset();
  }

  public void AddLicense()
  {
    if (string.IsNullOrEmpty(_publishingOptions.License?.Uri)) return;

    var licenseProps = _publishingOptions.License.Properties;
    var licenseEntity = new CreativeWork(
      identifier: _publishingOptions.License.Uri,
      properties: JsonSerializer.SerializeToNode(licenseProps)?.AsObject());

    // Bug in ROCrates.Net: CreativeWork class uses the base constructor so @type is Thing by default
    licenseEntity.SetProperty("@type", "CreativeWork");

    _crate.Add(licenseEntity);

    _crate.RootDataset.SetProperty("license", new Part { Id = licenseEntity.Id });
  }

  public void AddProfile()
  {
    var profileEntity = new Entity(identifier: _crateProfileOptions.Id);
    profileEntity.SetProperty("@type", _crateProfileOptions.Type);
    profileEntity.SetProperty("name", _crateProfileOptions.Name);
    _crate.Add(profileEntity);
  }

  public void AddMainEntity()
  {
    var workflowURI = GetWorkflowUrl();
    var workflowEntity = new Dataset(identifier: workflowURI);
    workflowEntity.SetProperty("name", _workflowOptions.Name);
    _crate.Entities.TryGetValue("https://w3id.org/trusted-wfrun-crate/0.3", out var profile);

    if (profile is not null)
    {
      workflowEntity.SetProperty("conformsTo", new Part
      {
        Id = profile.Id
      });
    }

    workflowEntity.SetProperty("distribution", new Part
    {
      Id = Url.Combine(_workflowOptions.BaseUrl, _workflowOptions.Id.ToString(), "ro_crate")
        .SetQueryParam("version", _workflowOptions.Version.ToString())
    });
    _crate.Add(workflowEntity);
    _crate.RootDataset.SetProperty("mainEntity", workflowEntity.Id);
  }

  /// <summary>
  /// <para>Add the <c>CreateAction</c> to the RO-Crate.</para>
  /// <para>This includes inputs necessary to run the <c>rquest-omop-worker</c>.</para>
  /// </summary>
  /// <param name="queryFileName">The name of the file where the RQuest query is saved.</param>
  /// <param name="isAvailability">Is the query an availability query? If not, treat as a distribution query.</param>
  public void AddCreateAction(string queryFileName, bool isAvailability)
  {
    var createActionId = $"#query-{Guid.NewGuid()}";
    var createAction = new ContextEntity(_crate, createActionId);
    createAction.SetProperty("@type", "CreateAction");
    createAction.SetProperty("actionStatus", ActionStatus.PotentialActionStatus);

    _crate.Entities.TryGetValue(GetWorkflowUrl(), out var workflow);
    if (workflow is not null) createAction.SetProperty("instrument", new Part { Id = workflow.Id });
    createAction.SetProperty("name", "RQuest Query");

    // set up OMOP worker inputs

    // body
    var body = AddQueryJsonMetadata(queryFileName);
    createAction.AppendTo("object", body);

    // is_availability
    var isAvailabilityEntity = AddQueryTypeMetadata(isAvailability);
    createAction.AppendTo("object", isAvailabilityEntity);

    _crate.Add(createAction);
  }

  private ROCrates.Models.File AddQueryJsonMetadata(string queryFileName)
  {
    var bodyParam = new ContextEntity(null, "#{_workflowOptions.Name}-inputs-body");
    bodyParam.SetProperty("@type", "FormalParameter");
    bodyParam.SetProperty("name", "body");
    bodyParam.SetProperty("dct:conformsTo", "https://bioschemas.org/profiles/FormalParameter/1.0-RELEASE/");
    var bodyEntity = new ROCrates.Models.File(null, queryFileName);
    bodyEntity.SetProperty("name", "rquest-query");
    bodyEntity.SetProperty("exampleOfWork", new Part { Id = bodyParam.Id });

    _crate.Add(bodyParam, bodyEntity);
    return bodyEntity;
  }

  private ContextEntity AddQueryTypeMetadata(bool isAvailability)
  {
    var paramId = "#{_workflowOptions.Name}-inputs-{0}";
    var entityId = "#input_{0}";

    var isAvailabilityParam =
      new ContextEntity(null, string.Format(paramId, isAvailability ? "is_availability" : "is_distribution"));
    isAvailabilityParam.SetProperty("@type", "FormalParameter");
    isAvailabilityParam.SetProperty("name", isAvailability ? "is_availability" : "is_distribution");
    isAvailabilityParam.SetProperty("dct:conformsTo", "https://bioschemas.org/profiles/FormalParameter/1.0-RELEASE/");
    var isAvailabilityEntity = new ContextEntity(null,
      string.Format(entityId, isAvailability ? "is_availability" : "is_distribution"));
    isAvailabilityEntity.SetProperty("@type", "PropertyValue");
    isAvailabilityEntity.SetProperty("name", isAvailability ? "is_availability" : "is_distribution");
    isAvailabilityEntity.SetProperty("value", isAvailability);
    isAvailabilityEntity.SetProperty("exampleOfWork", new Part { Id = isAvailabilityParam.Id });

    _crate.Add(isAvailabilityParam, isAvailabilityEntity);
    return isAvailabilityEntity;
  }

  public ROCrate GetROCrate()
  {
    ROCrate result = _crate;
    ResetCrate();
    return result;
  }

  public void ResetCrate()
  {
    _crate = new ROCrate();
    UpdateRootDataset();
  }

  private string GetWorkflowUrl()
  {
    return Url.Combine(_workflowOptions.BaseUrl, _workflowOptions.Id.ToString())
      .SetQueryParam("version", _workflowOptions.Version.ToString());
  }

  private void UpdateRootDataset()
  {
    _crate.RootDataset.SetProperty("conformsTo", new Part
    {
      Id = "https://w3id.org/trusted-wfrun-crate/0.3",
    });
    _crate.RootDataset.SetProperty("datePublished", DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture));
  }

  public void AddAgent()
  {
    var agentEntity = new Entity(identifier: _crateAgentOptions.Id);
    agentEntity.SetProperty("@type", _crateAgentOptions.Type);
    agentEntity.SetProperty("name", _crateAgentOptions.Name);
    agentEntity.SetProperty("affiliation", _crateAgentOptions.Affiliation);
    agentEntity.SetProperty("memberOf", new List<Part>
    {
      new() { Id = _crateProjectOptions.Id }
    });
    _crate.Add(agentEntity);
  }

  public void AddProject()
  {
    var projectEntity = new Entity(identifier: $"#project-{Guid.NewGuid()}");
    projectEntity.SetProperty("@type", _crateProjectOptions.Type);
    projectEntity.SetProperty("name", _crateProjectOptions.Name);
    projectEntity.SetProperty("identifier", _crateProjectOptions.Identifiers);
    projectEntity.SetProperty("funding", _crateProjectOptions.Funding);
    projectEntity.SetProperty("member", _crateProjectOptions.Member);
    _crate.Add(projectEntity);
  }

  public void AddOrganisation()
  {
    var orgEntity = new Entity(identifier: _crateOrganizationOptions.Id);
    orgEntity.SetProperty("@type", _crateOrganizationOptions.Type);
    orgEntity.SetProperty("name", _crateOrganizationOptions.Name);
    _crate.Add(orgEntity);
  }
}
