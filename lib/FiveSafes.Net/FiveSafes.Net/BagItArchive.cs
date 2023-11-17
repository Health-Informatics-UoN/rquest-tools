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

  public string Path => _archive.FullName;

  public string PayloadDirectoryPath => System.IO.Path.Combine(Path, _payloadDirectoryPath);

  /// <summary>
  /// Create the BagIt archive's payload directory, call <c>data</c>.
  /// </summary>
  /// <exception cref="IOException">The directory cannot be created.</exception>
  public void AddPayloadDirectory()
  {
    _archive.CreateSubdirectory(_payloadDirectoryPath);
  }

  /// <summary>
  /// Add a file to the BagIt archive's <c>data</c> directory. The file will be overwritten if it already exists.
  /// </summary>
  /// <param name="sourceFile">The file to add to the archive.</param>
  /// <exception cref="FileNotFoundException">The source file does not exist.</exception>
  public void AddFile(string sourceFile)
  {
    var sourceFileInfo = new FileInfo(sourceFile);
    if (!sourceFileInfo.Exists) throw new FileNotFoundException();
    var destination = System.IO.Path.Combine(PayloadDirectoryPath, sourceFileInfo.Name);
    sourceFileInfo.CopyTo(destination, overwrite: true);
  }
}
