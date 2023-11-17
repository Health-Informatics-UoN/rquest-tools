using FiveSafes.Net.Constants;
using FiveSafes.Net.Utilities;
using ROCrates;

namespace FiveSafes.Net;

public class FiveSafesBagItBuilder : IBagItArchiveBuilder
{
  private static string[] _tagFiles =
    { BagItConstants.BagitTxtPath, BagItConstants.BagInfoTxtPath, BagItConstants.ManifestPath };

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
    await WriteManifestSha512();
    await WriteTagManifestSha512();
  }

  /// <inheritdoc />
  public async Task BuildTagFiles()
  {
    await WriteBagitTxt();
    await WriteBagInfoTxt();
  }

  /// <inheritdoc />
  public void BuildPayloadDirectory()
  {
    var roCrate = new ROCrate();
    roCrate.Save(_archive.PayloadDirectoryPath);
  }

  /// <summary>
  /// Compute the SHA512 for each file in the Bagit archive's <c>data</c> subdirectory and write a
  /// <c>manifest-sha512.txt</c> to the archive.
  /// </summary>
  private async Task WriteManifestSha512()
  {
    await using var manifestFile =
      new FileStream(Path.Combine(_archive.Path, BagItConstants.ManifestPath), FileMode.Create, FileAccess.Write);
    await using var writer = new StreamWriter(manifestFile);
    foreach (var entry in Directory.EnumerateFiles(_archive.PayloadDirectoryPath, "*", SearchOption.AllDirectories))
    {
      await using var stream = new FileStream(entry, FileMode.Open, FileAccess.Read);
      var checksum = ChecksumUtility.ComputeSha512(stream);
      // Note there should be 2 spaces between the checksum and the file path
      // The path should be relative to bagitDir
      var path = Path.GetRelativePath(_archive.Path, entry);
      await writer.WriteLineAsync($"{checksum}  {path}");
    }
  }

  /// <summary>
  /// Compute the SHA512 for the <c>bagit.txt</c>, <c>bag-info.txt</c> and <c>manifest-sha512.txt</c> and
  /// write a <c>tagmanifest-sha512.txt</c> to the archive.
  /// </summary>
  /// <exception cref="FileNotFoundException">Thrown if a tag file doesn't exist in the archive.</exception>
  private async Task WriteTagManifestSha512()
  {
    await using var manifestFile =
      new FileStream(Path.Combine(_archive.Path, BagItConstants.TagManifestPath), FileMode.Create, FileAccess.Write);
    await using var writer = new StreamWriter(manifestFile);
    foreach (var tagFile in _tagFiles)
    {
      var filePath = Path.Combine(_archive.Path, tagFile);
      if (!File.Exists(filePath)) throw new FileNotFoundException(null, filePath);
      await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
      var checksum = ChecksumUtility.ComputeSha512(stream);
      // Note there should be 2 spaces between the checksum and the file path
      await writer.WriteLineAsync($"{checksum}  {tagFile}");
    }
  }

  private async Task WriteBagitTxt()
  {
    string[] contents = { "BagIt-Version: 1.0", "Tag-File-Character-Encoding: UTF-8" };
    await using var manifestFile =
      new FileStream(Path.Combine(_archive.Path, BagItConstants.BagitTxtPath), FileMode.Create, FileAccess.Write);
    await using var writer = new StreamWriter(manifestFile);
    foreach (var line in contents)
    {
      await writer.WriteLineAsync(line);
    }
  }

  private async Task WriteBagInfoTxt()
  {
    var contents = "External-Identifier: urn:uuid:{}";
    var line = string.Format(contents, Guid.NewGuid().ToString());
    await using var manifestFile =
      new FileStream(Path.Combine(_archive.Path, BagItConstants.BagInfoTxtPath), FileMode.Create, FileAccess.Write);
    await using var writer = new StreamWriter(manifestFile);
    await writer.WriteLineAsync(line);
  }
}
