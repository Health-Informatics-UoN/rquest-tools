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

  [Fact]
  public void AddFile_Throws_OnNonExistentSourceFile()
  {
    // Arrange
    const string nonExistentFile = "non-existent.txt";
    var archive = new BagItArchive(_bagItArchiveFixture.Dir.FullName);

    // Act
    var action = () => archive.AddFile(nonExistentFile);

    // Assert
    Assert.Throws<FileNotFoundException>(action);
  }

  [Fact]
  public void AddFile_Adds_SourceFileToPayload()
  {
    // Arrange
    var archive = new BagItArchive(_bagItArchiveFixture.Dir.FullName);
    Directory.CreateDirectory(archive.PayloadDirectoryPath);

    // Act
    archive.AddFile(_bagItArchiveFixture.TestFile.FullName);
    archive.AddFile(_bagItArchiveFixture.TestFile.FullName, toPayload: false);

    // Assert
    Assert.True(File.Exists(Path.Combine(archive.PayloadDirectoryPath, _bagItArchiveFixture.TestFile.Name)));
    Assert.True(File.Exists(Path.Combine(archive.ArchiveRootPath, _bagItArchiveFixture.TestFile.Name)));
  }
}

public class BagItArchiveFixture : IDisposable
{
  private const string _file = "test.txt";
  private readonly string _dir = Guid.NewGuid().ToString();

  public BagItArchiveFixture()
  {
    Dir = new DirectoryInfo(_dir);
    Dir.Create();
    TestFile = new FileInfo(_file);
    TestFile.Create().Close();
  }

  public DirectoryInfo Dir { get; }
  public FileInfo TestFile { get; }

  public void Dispose()
  {
    Dir.Delete(recursive: true);
    TestFile.Delete();
  }
}
