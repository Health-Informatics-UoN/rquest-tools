using FiveSafes.Net.Constants;

namespace FiveSafes.Net.Tests;

public class TestPacker : IClassFixture<PackerFixture>
{
  private readonly PackerFixture _packerFixture;

  public TestPacker(PackerFixture packerFixture)
  {
    _packerFixture = packerFixture;
  }

  [Fact]
  public async Task BuildBlankArchive_Creates_ExpectedStructure()
  {
    // Arrange
    var builder = new FiveSafesBagItBuilder(_packerFixture.Dir.FullName);
    var packer = new Packer(builder);

    // Act
    await packer.BuildBlankArchive();
    var archive = builder.GetArchive();

    // Assert
    Assert.True(Directory.Exists(archive.PayloadDirectoryPath));
    Assert.True(File.Exists(Path.Combine(archive.ArchiveRootPath, BagItConstants.ManifestPath)));
    Assert.True(File.Exists(Path.Combine(archive.ArchiveRootPath, BagItConstants.TagManifestPath)));
    Assert.True(File.Exists(Path.Combine(archive.ArchiveRootPath, BagItConstants.BagitTxtPath)));
    Assert.True(File.Exists(Path.Combine(archive.ArchiveRootPath, BagItConstants.BagInfoTxtPath)));
  }
}

public class PackerFixture : IDisposable
{
  private readonly string _dir = Guid.NewGuid().ToString();

  public PackerFixture()
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
