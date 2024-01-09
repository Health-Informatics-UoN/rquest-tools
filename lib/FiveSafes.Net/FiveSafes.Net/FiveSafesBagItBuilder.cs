using System.Globalization;
using FiveSafes.Net.Constants;
using ROCrates;
using ROCrates.Models;

namespace FiveSafes.Net;

public class FiveSafesBagItBuilder
{
  private readonly BagItArchive _archive;
  private readonly FiveSafesProfile _profile = new();
  private ROCrate _crate = new();

  /// <summary>
  /// Instantiates a blank BagIt archive in the current working directory.
  /// </summary>
  public FiveSafesBagItBuilder()
  {
    _archive = new BagItArchive(Directory.GetCurrentDirectory());
  }

  /// <summary>
  /// Instantiates a blank BagIt archive in the given directory.
  /// </summary>
  /// <param name="archiveDirectory">The directory to instantiate it in.</param>
  public FiveSafesBagItBuilder(string archiveDirectory)
  {
    _archive = new BagItArchive(archiveDirectory);
  }

  /// <summary>
  /// Load an existing BagItArchive. This is useful for modifying an existing archive.
  /// </summary>
  /// <param name="bagItArchive"></param>
  public FiveSafesBagItBuilder(BagItArchive bagItArchive)
  {
    _archive = bagItArchive;
  }

  public async Task BuildChecksums()
  {
    await _archive.WriteManifestSha512();
    await _archive.WriteTagManifestSha512();
  }

  public async Task BuildTagFiles()
  {
    await _archive.WriteBagitTxt();
    await _archive.WriteBagInfoTxt();
  }

  public BagItArchive GetArchive()
  {
    BagItArchive result = _archive;
    return result;
  }

  public void BuildCrate(string workflowUri)
  {
    AddRootDataset();
    AddProfile();
    AddMainEntity(workflowUri);
    _crate.Save(_archive.PayloadDirectoryPath);
  }

  /// <summary>
  /// Adds basic information for the metadata to ROCrate.
  /// </summary>
  private void AddRootDataset()
  {
    _crate.RootDataset.SetProperty("conformsTo", new Part
    {
      Id = _profile.Id,
    });
    _crate.RootDataset.SetProperty("datePublished", DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture));
  }

  private void AddProfile()
  {
    var profileEntity = new Entity(identifier: _profile.Id);
    profileEntity.SetProperty("@type", _profile.Type);
    profileEntity.SetProperty("name", _profile.Name);
    _crate.Add(profileEntity);
  }

  private void AddMainEntity(string workflowUri)
  {
    var workflowEntity = new Dataset(identifier: workflowUri);

    workflowEntity.SetProperty("conformsTo", new Part
    {
      Id = _profile.Id
    });

    _crate.Add(workflowEntity);
    _crate.RootDataset.SetProperty("mainEntity", new Part { Id = workflowEntity.Id });
  }
}
