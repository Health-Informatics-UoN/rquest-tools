namespace FiveSafes.Net;

public class BagItArchive
{
  private DirectoryInfo _archive;
  private string _payloadDirectoryPath = "data";

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

  public string PayloadDirectoryPath => Path.Combine(ArchiveRootPath, _payloadDirectoryPath);

  /// <summary>
  /// Create the BagIt archive's payload directory, call <c>data</c>.
  /// </summary>
  /// <exception cref="IOException">The directory cannot be created.</exception>
  public void AddPayloadDirectory()
  {
    _archive.CreateSubdirectory(_payloadDirectoryPath);
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
}
