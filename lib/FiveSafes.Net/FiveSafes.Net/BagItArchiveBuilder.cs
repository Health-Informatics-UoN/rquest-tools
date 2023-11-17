namespace FiveSafes.Net;

public class BagItArchiveBuilder : IBagItArchiveBuilder
{
  private BagItArchive _archive = new BagItArchive();

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
