using FiveSafes.Net.Constants;
using ROCrates;
using ROCrates.Models;

namespace FiveSafes.Net;

public class FiveSafesBagItBuilder
{
  private readonly BagItArchive _archive;
  private readonly FiveSafesProfile _profile = new();
  private ROCrate _crate = new();

  public FiveSafesBagItBuilder()
  {
    _archive = new BagItArchive(Directory.GetCurrentDirectory());
  }

  public FiveSafesBagItBuilder(string archiveDirectory)
  {
    _archive = new BagItArchive(archiveDirectory);
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
    AddProfile();
    AddMainEntity(workflowUri);
    _crate.Save(_archive.PayloadDirectoryPath);
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
