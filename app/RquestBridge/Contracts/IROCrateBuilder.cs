using ROCrates.Models;

namespace RquestBridge.Contracts;

public interface IROCrateBuilder
{
  /// <summary>
  /// Add a Profile entity to the RO-Crate.
  /// </summary>
  public void AddProfile();

  /// <summary>
  /// Add a Licesne entity to the RO-Crate.
  /// </summary>
  public void AddLicense();

  /// <summary>
  /// Add the main entity to the RO-Crate.
  /// </summary>
  public void AddMainEntity();
}
