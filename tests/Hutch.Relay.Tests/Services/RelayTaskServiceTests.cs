using Hutch.Relay.Data;
using Hutch.Relay.Data.Entities;
using Hutch.Relay.Models;
using Hutch.Relay.Services;
using Xunit;
using Assert = Xunit.Assert;

namespace Hutch.Relay.Tests.Services;

public class RelayTaskServiceTests(Fixtures fixtures) : IClassFixture<Fixtures>
{
  private readonly ApplicationDbContext _dbContext = fixtures.DbContext;
  
  [Fact]
  public async Task Get_WithValidId_ReturnsRelayTaskModel()
  {
    // Arrange
    var relayTask = new RelayTask
    {
      Id = "valid-id",
      CreatedAt = DateTime.UtcNow,
      Collection = "Sample Collection"
    };
    _dbContext.RelayTasks.Add(relayTask);
    await _dbContext.SaveChangesAsync();

    var service = new RelayTaskService(_dbContext);

    // Act
    var result = await service.Get("valid-id");

    // Assert
    Assert.NotNull(result);
    Assert.Equal("valid-id", result.Id);
    Assert.Equal(relayTask.CreatedAt, result.CreatedAt);
    Assert.Equal(relayTask.Collection, result.Collection);
  }

  [Fact]
  public async Task Get_WithInvalidId_ThrowsKeyNotFoundException()
  {
    // Arrange
    var service = new RelayTaskService(_dbContext);
  
    // Act / Assert
    await Assert.ThrowsAsync<KeyNotFoundException>(() => service.Get("DoesNotExist"));
  }
  
  [Fact]
  public async Task Create_ValidRelayTaskModel_ReturnsCreatedRelayTaskModel()
  {
    // Arrange
    var model = new RelayTaskModel
    {
      Collection = "New Collection"
    };

    var service = new RelayTaskService(_dbContext);

    // Act
    var result = await service.Create(model);

    // Assert
    Assert.NotNull(result);
    Assert.NotNull(result.Id);
    Assert.Equal(model.Collection, result.Collection);
    Assert.Null(result.CompletedAt);

    var entityInDb = await _dbContext.RelayTasks.FindAsync(result.Id);
    Assert.NotNull(entityInDb);
    Assert.Equal(model.Collection, entityInDb.Collection);
  }
}
