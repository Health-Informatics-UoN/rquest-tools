namespace FiveSafes.Net;

public interface IBagItArchiveBuilder
{
  /// <summary>
  /// Compute the checksums for the <c>manifest-sha512.txt</c> and <c>tagmanifest-sha512.txt</c> files
  /// and add them to the archive.
  /// </summary>
  Task BuildChecksums();

  /// <summary>
  /// Build the RO-Crate that will go in the BagIt archive's <c>data</c> directory.
  /// </summary>
  Task BuildRoCrate();
}
