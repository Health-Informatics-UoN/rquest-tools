using FiveSafes.Net.Constants;
using FiveSafes.Net.Utilities;

namespace FiveSafes.Net;

public class BagItArchive
{
  private static string[] _tagFiles =
    { BagItConstants.BagitTxtPath, BagItConstants.BagInfoTxtPath, BagItConstants.ManifestPath };

  private DirectoryInfo _archive;

  /// <summary>
  /// Create a <c>BagItArchive</c> in the given directory.
  /// </summary>
  /// <param name="archiveDirectory">The path to the archive.</param>
  public BagItArchive(string archiveDirectory)
  {
    _archive = new DirectoryInfo(archiveDirectory);
    if (!_archive.Exists) _archive.Create();
  }

  public string ArchiveRootPath => _archive.FullName;

  public string PayloadDirectoryPath => Path.Combine(ArchiveRootPath, BagItConstants.PayloadDirectory);

  /// <summary>
  /// Create the BagIt archive's payload directory, call <c>data</c>.
  /// </summary>
  /// <exception cref="IOException">The directory cannot be created.</exception>
  public void AddPayloadDirectory()
  {
    _archive.CreateSubdirectory(BagItConstants.PayloadDirectory);
  }

  /// <summary>
  /// Add a file to the BagIt archive. The file will be overwritten if it already exists.
  /// </summary>
  /// <param name="sourceFile">The file to add to the archive.</param>
  /// <param name="toPayload">Add file to payload directory? Default: <c>true</c>.</param>
  /// <exception cref="FileNotFoundException">The source file does not exist.</exception>
  public void AddFile(string sourceFile, bool toPayload = true)
  {
    var sourceFileInfo = new FileInfo(sourceFile);
    if (!sourceFileInfo.Exists) throw new FileNotFoundException();
    var destination = Path.Combine(toPayload ? PayloadDirectoryPath : ArchiveRootPath, sourceFileInfo.Name);
    sourceFileInfo.CopyTo(destination, overwrite: true);
  }

  /// <summary>
  /// Compute the SHA512 for each file in the Bagit archive's <c>data</c> subdirectory and write a
  /// <c>manifest-sha512.txt</c> to the archive.
  /// </summary>
  public async Task WriteManifestSha512()
  {
    await using var manifestFile =
      new FileStream(Path.Combine(ArchiveRootPath, BagItConstants.ManifestPath), FileMode.Create,
        FileAccess.Write);
    if (!Directory.Exists(PayloadDirectoryPath)) return;
    await using var writer = new StreamWriter(manifestFile);
    foreach (var entry in Directory.EnumerateFiles(PayloadDirectoryPath, "*", SearchOption.AllDirectories))
    {
      await using var stream = new FileStream(entry, FileMode.Open, FileAccess.Read);
      var checksum = ChecksumUtility.ComputeSha512(stream);
      // Note there should be 2 spaces between the checksum and the file path
      // The path should be relative to bagitDir
      var path = Path.GetRelativePath(ArchiveRootPath, entry);
      await writer.WriteLineAsync($"{checksum}  {path}");
    }
  }

  /// <summary>
  /// Compute the SHA512 for the <c>bagit.txt</c>, <c>bag-info.txt</c> and <c>manifest-sha512.txt</c> and
  /// write a <c>tagmanifest-sha512.txt</c> to the archive.
  /// </summary>
  public async Task WriteTagManifestSha512()
  {
    await using var manifestFile =
      new FileStream(Path.Combine(ArchiveRootPath, BagItConstants.TagManifestPath), FileMode.Create,
        FileAccess.Write);
    await using var writer = new StreamWriter(manifestFile);
    foreach (var tagFile in _tagFiles)
    {
      var filePath = Path.Combine(ArchiveRootPath, tagFile);
      if (!File.Exists(filePath)) continue;
      await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
      var checksum = ChecksumUtility.ComputeSha512(stream);
      // Note there should be 2 spaces between the checksum and the file path
      await writer.WriteLineAsync($"{checksum}  {tagFile}");
    }
  }

  public async Task WriteBagitTxt()
  {
    string[] contents = { "BagIt-Version: 1.0", "Tag-File-Character-Encoding: UTF-8" };
    await using var manifestFile =
      new FileStream(Path.Combine(ArchiveRootPath, BagItConstants.BagitTxtPath), FileMode.Create,
        FileAccess.Write);
    await using var writer = new StreamWriter(manifestFile);
    foreach (var line in contents)
    {
      await writer.WriteLineAsync(line);
    }
  }

  public async Task WriteBagInfoTxt()
  {
    var contents = "External-Identifier: urn:uuid:{0}";
    var line = string.Format(contents, Guid.NewGuid().ToString());
    await using var manifestFile =
      new FileStream(Path.Combine(ArchiveRootPath, BagItConstants.BagInfoTxtPath), FileMode.Create,
        FileAccess.Write);
    await using var writer = new StreamWriter(manifestFile);
    await writer.WriteLineAsync(line);
  }
}
