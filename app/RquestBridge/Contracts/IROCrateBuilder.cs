namespace RquestBridge.Contracts;

public interface IROCrateBuilder
{
  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  void AddProfile();

  void AddRootDataset();

  void AddLicense();

  void AddCreateAction();

  void AddWorkflow();
}
