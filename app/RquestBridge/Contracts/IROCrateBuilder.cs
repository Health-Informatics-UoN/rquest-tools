using ROCrates.Models;

namespace RquestBridge.Contracts;

public interface IROCrateBuilder
{
  public void AddProfile();

  public void AddLicense();

  public void AddWorkflow();
}
