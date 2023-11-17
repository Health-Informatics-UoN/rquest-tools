using FiveSafes.Net.Utilities;

namespace FiveSafes.Net;

public class BagItArchiveBuilder : IBagItArchiveBuilder
{
  private const string _manifestName = "manifest-sha512.txt";
  private const string _tagManifestName = "tagmanifest-sha512.txt";

  private static string[] _tagFiles =
    { "bagit.txt", "bag-info.txt", "manifest-sha512.txt" };

  private BagItArchive _archive;

  public BagItArchiveBuilder()
  {
    _archive = new BagItArchive(Directory.GetCurrentDirectory());
  }

  public BagItArchiveBuilder(string archiveDirectory)
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
  public async Task BuildRoCrate()
  {
    throw new NotImplementedException();
  }

  /// <summary>
  /// Compute the SHA512 for each file in the Bagit archive's <c>data</c> subdirectory and write a
  /// <c>manifest-sha512.txt</c> to the archive.
  /// </summary>
  private async Task WriteManifestSha512()
  {
    await using var manifestFile =
      new FileStream(Path.Combine(_archive.Path, _manifestName), FileMode.Create, FileAccess.Write);
    await using var writer = new StreamWriter(manifestFile);
    foreach (var entry in Directory.EnumerateFiles(_archive.DataDirectoryPath, "*", SearchOption.AllDirectories))
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
      new FileStream(Path.Combine(_archive.Path, _tagManifestName), FileMode.Create, FileAccess.Write);
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
}
