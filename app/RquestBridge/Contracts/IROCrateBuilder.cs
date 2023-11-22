using ROCrates.Models;

namespace RquestBridge.Contracts;

public interface IROCrateBuilder
{
  public void AddProfile();

  public void AddRootDataset();

  public void AddLicense();

  public void AddCreateAction();

  public void AddWorkflow();
}
