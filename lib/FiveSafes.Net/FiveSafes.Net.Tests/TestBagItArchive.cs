namespace FiveSafes.Net.Tests;

public class TestBagItArchive : IClassFixture<BagItArchiveFixture>
{
  private readonly BagItArchiveFixture _bagItArchiveFixture;

  public TestBagItArchive(BagItArchiveFixture bagItArchiveFixture)
  {
    _bagItArchiveFixture = bagItArchiveFixture;
  }

  [Fact]
  public void AddPayloadDirectory_Creates_PayloadDirectory()
  {
    // Arrange
    var archive = new BagItArchive(_bagItArchiveFixture.Dir.FullName);

    // Act
    archive.AddPayloadDirectory();

    // Assert
    Assert.True(Directory.Exists(archive.PayloadDirectoryPath));
  }
}

public class BagItArchiveFixture : IDisposable
{
  private readonly string _dir = Guid.NewGuid().ToString();

  public BagItArchiveFixture()
  {
    Dir = new DirectoryInfo(_dir);
    Dir.Create();
  }

  public DirectoryInfo Dir { get; }

  public void Dispose()
  {
    Dir.Delete(recursive: true);
  }
}
