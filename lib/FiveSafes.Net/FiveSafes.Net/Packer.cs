namespace FiveSafes.Net;

public class Packer
{
  private readonly IBagItArchiveBuilder _builder;

  public Packer(IBagItArchiveBuilder builder)
  {
    _builder = builder;
  }

  /// <summary>
  /// Build a "blank" BagItArchive containing only the tag files, the manifest files and payload directory.
  /// </summary>
  public async Task BuildBlankArchive()
  {
    _builder.BuildPayloadDirectory();
    await _builder.BuildTagFiles();
    await _builder.BuildChecksums();
  }
}
