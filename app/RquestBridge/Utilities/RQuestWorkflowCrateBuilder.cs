using System.Text.Json;
using Flurl;
using ROCrates;
using ROCrates.Models;
using RquestBridge.Config;
using RquestBridge.Constants;

namespace RquestBridge.Utilities;

public class RQuestWorkflowCrateBuilder
{
  private readonly AgreementPolicyOptions _agreementPolicy;
  private readonly CrateAgentOptions _crateAgentOptions;
  private readonly CrateOrganizationOptions _crateOrganizationOptions;
  private readonly CrateProjectOptions _crateProjectOptions;
  private readonly CratePublishingOptions _publishingOptions;
  private readonly WorkflowOptions _workflowOptions;
  private ROCrate _crate = new ROCrate();

  public RQuestWorkflowCrateBuilder(WorkflowOptions workflowOptions, CratePublishingOptions publishingOptions,
    CrateAgentOptions crateAgentOptions, CrateProjectOptions crateProjectOptions,
    CrateOrganizationOptions crateOrganizationOptions,
    string archivePayloadDirectoryPath, AgreementPolicyOptions agreementPolicy)
  {
    _workflowOptions = workflowOptions;
    _publishingOptions = publishingOptions;
    _crateAgentOptions = crateAgentOptions;
    _crateProjectOptions = crateProjectOptions;
    _crateOrganizationOptions = crateOrganizationOptions;
    _agreementPolicy = agreementPolicy;

    _crate.Initialise(archivePayloadDirectoryPath);
    AddProject();
    AddOrganisation();
  }

  public RQuestWorkflowCrateBuilder(WorkflowOptions workflowOptions, CratePublishingOptions publishingOptions,
    CrateAgentOptions crateAgentOptions, CrateProjectOptions crateProjectOptions,
    CrateOrganizationOptions crateOrganizationOptions, AgreementPolicyOptions agreementPolicy)
  {
    _workflowOptions = workflowOptions;
    _publishingOptions = publishingOptions;
    _crateAgentOptions = crateAgentOptions;
    _crateProjectOptions = crateProjectOptions;
    _crateOrganizationOptions = crateOrganizationOptions;
    _agreementPolicy = agreementPolicy;

    AddProject();
    AddOrganisation();
  }

  /// <summary>
  /// Adds Licence Entity to Five Safes RO-Crate.
  /// </summary>
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

  /// <summary>
  /// Update mainEntity to Five Safes RO-Crate.
  /// </summary>
  /// <exception cref="InvalidDataException">mainEntity not found in RO-Crate.</exception>
  public void UpdateMainEntity()
  {
    var workflowUri = GetWorkflowUrl();
    if (_crate.Entities.TryGetValue(workflowUri, out var mainEntity))
    {
      mainEntity.SetProperty("name", _workflowOptions.Name);

      mainEntity.SetProperty("distribution", new Part
      {
        Id = Url.Combine(_workflowOptions.BaseUrl, _workflowOptions.Id.ToString(), "ro_crate")
          .SetQueryParam("version", _workflowOptions.Version.ToString())
      });
      _crate.Add(mainEntity);
      _crate.RootDataset.SetProperty("mainEntity", new Part { Id = mainEntity.Id });
    }
    else
    {
      throw new InvalidDataException("Could not find mainEntity in RO-Crate.");
    }
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

  /// <summary>
  /// Add the metadata elements for the query file.
  /// </summary>
  /// <param name="queryFileName">The name of the file where the query is saved.</param>
  /// <returns>The entity representing the query file.</returns>
  private ROCrates.Models.File AddQueryJsonMetadata(string queryFileName)
  {
    var bodyParam = new ContextEntity(null, $"#{_workflowOptions.Name}-inputs-body");
    bodyParam.SetProperty("@type", "FormalParameter");
    bodyParam.SetProperty("name", "body");
    bodyParam.SetProperty("dct:conformsTo", "https://bioschemas.org/profiles/FormalParameter/1.0-RELEASE/");
    var bodyEntity = new ROCrates.Models.File(null, queryFileName);
    bodyEntity.SetProperty("name", "rquest-query");
    bodyEntity.SetProperty("exampleOfWork", new Part { Id = bodyParam.Id });

    _crate.Add(bodyParam, bodyEntity);
    return bodyEntity;
  }

  /// <summary>
  /// Add the metadata elements concerning whether the query is an availability or a
  /// distribution query.
  /// </summary>
  /// <param name="isAvailability">Is the query an availability query.</param>
  /// <returns>The entity saying if the query is an availability or distribution query.</returns>
  private ContextEntity AddQueryTypeMetadata(bool isAvailability)
  {
    var paramId = "#{0}-inputs-{1}";
    var entityId = "#input_{0}";

    var isAvailabilityParam =
      new ContextEntity(null,
        string.Format(paramId, _workflowOptions.Name, isAvailability ? "is_availability" : "is_distribution"));
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

  /// <summary>
  ///  Returns the ROCrate.
  /// </summary>
  /// <returns>resulting ROCrate</returns>
  public ROCrate GetROCrate()
  {
    ROCrate result = _crate;
    ResetCrate();
    return result;
  }

  /// <summary>
  /// Resets the ROCrate back to a blank ROCrate.
  /// </summary>
  private void ResetCrate()
  {
    _crate = new ROCrate();
  }

  /// <summary>
  /// Adds Agent Entity and links to relevant organisation and project.
  /// </summary>
  public void AddAgent()
  {
    var organisation = _crate.Entities.Values.First(x => x.Id == _crateOrganizationOptions.Id);
    var project = _crate.Entities.Values.First(x => x.Id.StartsWith("#project-"));
    var agentEntity = new Entity(identifier: _crateAgentOptions.Id);
    agentEntity.SetProperty("@type", _crateAgentOptions.Type);
    agentEntity.SetProperty("name", _crateAgentOptions.Name);
    agentEntity.SetProperty("affiliation", new Part() { Id = organisation.Id });
    agentEntity.AppendTo("memberOf", project);
    _crate.Add(agentEntity);
  }

  /// <summary>
  /// Adds Project Entity as configured
  /// </summary>
  /// <returns></returns>
  private void AddProject()
  {
    var projectEntity = new Entity(identifier: $"#project-{Guid.NewGuid()}");
    projectEntity.SetProperty("@type", _crateProjectOptions.Type);
    projectEntity.SetProperty("name", _crateProjectOptions.Name);
    projectEntity.SetProperty("identifier", _crateProjectOptions.Identifiers);
    projectEntity.SetProperty("funding", _crateProjectOptions.Funding);
    projectEntity.SetProperty("member", _crateProjectOptions.Member);
    _crate.Add(projectEntity);
  }

  /// <summary>
  /// Adds Organisation Entity as configured.
  /// </summary>
  /// <returns></returns>
  private void AddOrganisation()
  {
    var orgEntity = new Entity(identifier: _crateOrganizationOptions.Id);
    orgEntity.SetProperty("@type", _crateOrganizationOptions.Type);
    orgEntity.SetProperty("name", _crateOrganizationOptions.Name);
    _crate.Add(orgEntity);
  }

  public void AddCheckValueAssessAction(string status, DateTime startTime, Part validator)
  {
    var checkActionId = $"#check-{Guid.NewGuid()}";
    var checkAction = new ContextEntity(_crate, checkActionId);
    checkAction.SetProperty("startTime", startTime);
    checkAction.SetProperty("@type", "AssessAction");
    checkAction.SetProperty("additionalType", new Part() { Id = "https://w3id.org/shp#CheckValue" });
    var statusMsg = GetStatus(status);
    checkAction.SetProperty("name", $"BagIt checksum of Crate: {statusMsg}");
    checkAction.SetProperty("actionStatus", status);
    checkAction.SetProperty("object", new Part { Id = _crate.RootDataset.Id });

    var instrument = new Entity { Id = "https://www.iana.org/assignments/named-information#sha-512" };
    instrument.SetProperty("@type", "DefinedTerm");
    instrument.SetProperty("name", "sha-512 algorithm");
    checkAction.SetProperty("instrument", new Part() { Id = instrument.Id });
    checkAction.SetProperty("agent", validator);
    checkAction.SetProperty("endTime", DateTime.Now);
    _crate.RootDataset.AppendTo("mentions", checkAction);
    _crate.Add(checkAction, instrument);
  }

  public void AddValidateCheck(string status, Part validator)
  {
    var profile = _crate.RootDataset.GetProperty<Part>("conformsTo") ??
                  throw new NullReferenceException("No profile found in RootDataset");

    var validateId = $"#validate - {Guid.NewGuid()}";
    var validateAction = new ContextEntity(_crate, validateId);
    validateAction.SetProperty("startTime", DateTime.Now);
    validateAction.SetProperty("@type", "AssessAction");
    validateAction.SetProperty("additionalType", new Part() { Id = "https://w3id.org/shp#ValidationCheck" });

    validateAction.SetProperty("name", $"Validation against Five Safes RO-Crate profile: approved");
    validateAction.SetProperty("actionStatus", status);
    validateAction.SetProperty("object", new Part { Id = _crate.RootDataset.Id });
    validateAction.SetProperty("instrument", new Part() { Id = profile.Id });
    validateAction.SetProperty("agent", validator);
    validateAction.SetProperty("endTime", DateTime.Now);
    _crate.RootDataset.AppendTo("mentions", validateAction);

    _crate.Add(validateAction);
  }

  public void AddSignOff()
  {
    var signOffEntity = new ContextEntity(identifier: $"#signoff-{Guid.NewGuid()}");
    signOffEntity.SetProperty("@type", "AssessAction");
    signOffEntity.SetProperty("additionalType", new Part { Id = "https://w3id.org/shp#SignOff" });
    signOffEntity.SetProperty("name", "Sign-off of execution according to Agreement policy");
    signOffEntity.SetProperty("endTime", DateTime.Now);
    _crate.Entities.TryGetValue(_crateAgentOptions.Id, out var agent);
    signOffEntity.SetProperty("agent", new Part() { Id = agent!.Id });
    var projectId = _crate.Entities.Keys.First(x => x.StartsWith("#project-"));
    signOffEntity.SetProperty("object", new Part[]
    {
      new() { Id = _crate.RootDataset.Id },
      new() { Id = GetWorkflowUrl() },
      new() { Id = projectId },
    });
    signOffEntity.SetProperty("actionStatus", ActionStatus.CompletedActionStatus);
    var agreementPolicyEntity = new CreativeWork(identifier: _agreementPolicy.Id);
    signOffEntity.SetProperty("instrument", new Part { Id = _agreementPolicy.Id });
    // Manually set type due to bug in ROCrates.Net
    agreementPolicyEntity.SetProperty("@type", "CreativeWork");
    agreementPolicyEntity.SetProperty("name", _agreementPolicy.Name);

    _crate.RootDataset.AppendTo("mentions", signOffEntity);
    _crate.Add(signOffEntity, agreementPolicyEntity);
  }

  /// <summary>
  /// Construct the Workflow URL from WorkflowOptions.
  /// </summary>
  /// <returns>Workflow URL</returns>
  public string GetWorkflowUrl()
  {
    return Url.Combine(_workflowOptions.BaseUrl, _workflowOptions.Id.ToString())
      .SetQueryParam("version", _workflowOptions.Version.ToString());
  }

  private string GetStatus(string status)
  {
    return status switch
    {
      ActionStatus.CompletedActionStatus => "completed",
      ActionStatus.ActiveActionStatus => "active",
      ActionStatus.FailedActionStatus => "failed",
      ActionStatus.PotentialActionStatus => "potential",
      _ => ""
    };
  }
}
