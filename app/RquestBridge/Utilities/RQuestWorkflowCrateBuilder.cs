using System.Text.Json;
using Flurl;
using ROCrates;
using ROCrates.Models;
using RquestBridge.Config;
using RquestBridge.Constants;

namespace RquestBridge.Utilities;

public class RQuestWorkflowCrateBuilder(WorkflowOptions workflowOptions, CratePublishingOptions publishingOptions,
  CrateAgentOptions crateAgentOptions, CrateProjectOptions crateProjectOptions,
  CrateOrganizationOptions crateOrganizationOptions, CrateProfileOptions crateProfileOptions)
{
  private ROCrate _crate = new ROCrate();

  /// <summary>
  /// Adds Licence Entity to Five Safes RO-Crate.
  /// </summary>
  public void AddLicense()
  {
    if (string.IsNullOrEmpty(publishingOptions.License?.Uri)) return;

    var licenseProps = publishingOptions.License.Properties;
    var licenseEntity = new CreativeWork(
      identifier: publishingOptions.License.Uri,
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
      mainEntity.SetProperty("name", workflowOptions.Name);

      _crate.Entities.TryGetValue(crateProfileOptions.Id, out var profile);

      mainEntity.SetProperty("distribution", new Part
      {
        Id = Url.Combine(workflowOptions.BaseUrl, workflowOptions.Id.ToString(), "ro_crate")
          .SetQueryParam("version", workflowOptions.Version.ToString())
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
    var bodyParam = new ContextEntity(null, $"#{workflowOptions.Name}-inputs-body");
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
        string.Format(paramId, workflowOptions.Name, isAvailability ? "is_availability" : "is_distribution"));
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
    var organisation = AddOrganisation();
    var project = AddProject();
    var agentEntity = new Entity(identifier: crateAgentOptions.Id);
    agentEntity.SetProperty("@type", crateAgentOptions.Type);
    agentEntity.SetProperty("name", crateAgentOptions.Name);
    agentEntity.SetProperty("affiliation", new Part() { Id = organisation.Id });
    agentEntity.AppendTo("memberOf", project);
    _crate.Add(agentEntity, organisation, project);
  }

  /// <summary>
  /// Adds Project Entity as configured
  /// </summary>
  /// <returns></returns>
  private Entity AddProject()
  {
    var projectEntity = new Entity(identifier: $"#project-{Guid.NewGuid()}");
    projectEntity.SetProperty("@type", crateProjectOptions.Type);
    projectEntity.SetProperty("name", crateProjectOptions.Name);
    projectEntity.SetProperty("identifier", crateProjectOptions.Identifiers);
    projectEntity.SetProperty("funding", crateProjectOptions.Funding);
    projectEntity.SetProperty("member", crateProjectOptions.Member);
    return projectEntity;
  }

  /// <summary>
  /// Adds Organisation Entity as configured.
  /// </summary>
  /// <returns></returns>
  private Entity AddOrganisation()
  {
    var orgEntity = new Entity(identifier: crateOrganizationOptions.Id);
    orgEntity.SetProperty("@type", crateOrganizationOptions.Type);
    orgEntity.SetProperty("name", crateOrganizationOptions.Name);
    return orgEntity;
  }

  /// <summary>
  /// Construct the Workflow URL from WorkflowOptions.
  /// </summary>
  /// <returns>Workflow URL</returns>
  public string GetWorkflowUrl()
  {
    return Url.Combine(workflowOptions.BaseUrl, workflowOptions.Id.ToString())
      .SetQueryParam("version", workflowOptions.Version.ToString());
  }
}
