using RquestBridge.Contracts;
using ROCrates.Models;
using System.Globalization;
using RquestBridge.Config;
using Microsoft.Extensions.Options;
using Flurl;
using ROCrates;
using System.Text.Json;
using RquestBridge.Constants;
namespace RquestBridge.Utilities;

public class ROCrateBuilder : IROCrateBuilder
{
  private readonly WorkflowOptions _workflowOptions;
  private readonly CratePublishingOptions _publishingOptions;
  private ROCrate _crate = new ROCrate();

  public ROCrateBuilder(IOptions<WorkflowOptions> workflowOptions, IOptions<CratePublishingOptions> publishingOptions)
  {
    _workflowOptions = workflowOptions.Value;
    _publishingOptions = publishingOptions.Value;

    // Add 5 Safes props to RootDataset
    UpdateRootDataset();
  }

  public void AddCreateAction()
  {
    var createActionId = $"#query-{Guid.NewGuid()}";
    var createAction = new ContextEntity(_crate, createActionId);
    createAction.SetProperty("@type", "CreateAction");
    createAction.SetProperty("actionStatus", ActionStatus.PotentialActionStatus);

    _crate.Entities.TryGetValue(GetWorkflowUrl(), out var workflow);
    if (workflow is not null) createAction.SetProperty("instrument", new Part { Id = workflow.Id });
    createAction.SetProperty("name", "RQuest Query");
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
    var profileEntity = new Entity(identifier: "https://w3id.org/trusted-wfrun-crate/0.3");
    profileEntity.SetProperty("@type", "Profile");
    profileEntity.SetProperty("name", "Trusted Workflow Run Crate profile");
    _crate.Add(profileEntity);
  }

  public void AddWorkflow()
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
      Id = Url.Combine(_workflowOptions.BaseUrl, _workflowOptions.Id.ToString(), "ro_crate").SetQueryParam("version", _workflowOptions.Version.ToString())
    });
    _crate.Add(workflowEntity);
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
    return Url.Combine(_workflowOptions.BaseUrl, _workflowOptions.Id.ToString()).SetQueryParam("version", _workflowOptions.Version.ToString());
  }

  private void UpdateRootDataset()
  {
    _crate.RootDataset.SetProperty("conformsTo", new Part
    {
      Id = "https://w3id.org/trusted-wfrun-crate/0.3",
    });
    _crate.RootDataset.SetProperty("datePublished", DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture));
  }
}
