using ROCrates;

namespace FiveSafes.Net;

public class FiveSafesBagItBuilder : IBagItArchiveBuilder
{
  private BagItArchive _archive;

  public FiveSafesBagItBuilder()
  {
    _archive = new BagItArchive(Directory.GetCurrentDirectory());
  }

  public FiveSafesBagItBuilder(string archiveDirectory)
  {
    _archive = new BagItArchive(archiveDirectory);
  }

  /// <inheritdoc />
  public async Task BuildChecksums()
  {
    await _archive.WriteManifestSha512();
    await _archive.WriteTagManifestSha512();
  }

  /// <inheritdoc />
  public async Task BuildTagFiles()
  {
    await _archive.WriteBagitTxt();
    await _archive.WriteBagInfoTxt();
  }

  /// <inheritdoc />
  public void BuildPayloadDirectory()
  {
    var roCrate = new ROCrate();
    roCrate.Save(_archive.PayloadDirectoryPath);
  }

  public BagItArchive GetArchive()
  {
    BagItArchive result = _archive;
    return result;
  }
}
