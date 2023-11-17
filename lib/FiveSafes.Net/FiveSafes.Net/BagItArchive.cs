namespace FiveSafes.Net;

public class BagItArchive
{
  /// <summary>
  /// Create a the BagIt archive's <c>data</c> directory inside <c>directory</c>.
  /// </summary>
  /// <param name="directory">The BagIt directory.</param>
  /// <exception cref="NotImplementedException"></exception>
  public void AddDataDirectory(string directory)
  {
    throw new NotImplementedException();
  }

  /// <inheritdoc cref="AddDataDirectory(string)"/>
  public void AddDataDirectory(DirectoryInfo directory)
  {
    throw new NotImplementedException();
  }

  /// <summary>
  /// Add a file to the BagIt archive's <c>data</c> directory
  /// </summary>
  /// <param name="sourceFile"></param>
  public void AddFile(string sourceFile)
  {
    throw new NotImplementedException();
  }

  /// <inheritdoc cref="AddFile(string)"/>
  public void AddFile(FileInfo sourceFile)
  {
    throw new NotImplementedException();
  }
}
