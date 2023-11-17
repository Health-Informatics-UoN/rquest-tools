namespace FiveSafes.Net;

public class BagItArchive
{
  private DirectoryInfo _archive;
  private string _dataDirectoryPath = "data";

  /// <summary>
  /// Create a <c>BagItArchive</c> in the given directory.
  /// </summary>
  /// <param name="archiveDirectory">The path to the archive.</param>
  public BagItArchive(string archiveDirectory)
  {
    _archive = new DirectoryInfo(archiveDirectory);
    if (!_archive.Exists) _archive.Create();
  }

  /// <summary>
  /// Create the BagIt archive's <c>data</c> directory.
  /// </summary>
  /// <exception cref="IOException">The directory cannot be created.</exception>
  public void AddDataDirectory()
  {
    _archive.CreateSubdirectory(_dataDirectoryPath);
  }

  /// <summary>
  /// Add a file to the BagIt archive's <c>data</c> directory. The file will be overwritten if it already exists.
  /// </summary>
  /// <param name="sourceFile">The file to add to the archive.</param>
  public void AddFile(string sourceFile)
  {
    var sourceFileInfo = new FileInfo(sourceFile);
    if (!sourceFileInfo.Exists) return;
    sourceFileInfo.CopyTo(_archive.FullName, overwrite: true);
  }
}
