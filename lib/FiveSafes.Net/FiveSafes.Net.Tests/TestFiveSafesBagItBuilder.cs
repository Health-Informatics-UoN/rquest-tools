using ROCrates;
using ROCrates.Models;

namespace FiveSafes.Net.Tests;

public class TestFiveSafesBagItBuilder
{
  [Fact]
  public void BuildCrate_Adds_MainEntityAsConfigured()
  {
    // Arrange
    var workflowUri = "https://workflowhub.eu/workflows/289?version=1";
    var builder = new FiveSafesBagItBuilder();

    var expectedMainEntityPart = new Part { Id = workflowUri };

    // Act
    builder.BuildCrate(workflowUri);
    var archive = builder.GetArchive();
    var crate = new ROCrate();
    crate.Initialise(archive.PayloadDirectoryPath);
    crate.Entities.TryGetValue(workflowUri, out var mainEntity);
    var actualMainEntityPart = crate.RootDataset.GetProperty<Part>("mainEntity");

    // Assert
    Assert.NotNull(mainEntity);
    Assert.Equal(workflowUri, mainEntity.Id);
    Assert.NotNull(mainEntity.GetProperty<Part>("conformsTo"));
    Assert.NotNull(actualMainEntityPart);
    Assert.Equal(expectedMainEntityPart.Id, actualMainEntityPart.Id);
  }
}
