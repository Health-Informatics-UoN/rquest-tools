namespace FiveSafes.Net;

public class BagItArchiveBuilder : IBagItArchiveBuilder
{
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
  public void BuildChecksums()
  {
    throw new NotImplementedException();
  }

  /// <inheritdoc />
  public void BuildRoCrate()
  {
    throw new NotImplementedException();
  }
}
