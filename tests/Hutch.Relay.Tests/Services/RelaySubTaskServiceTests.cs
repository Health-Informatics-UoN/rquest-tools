using Hutch.Relay.Data;
using Hutch.Relay.Data.Entities;
using Hutch.Relay.Models;
using Hutch.Relay.Services;
using Xunit;
using Assert = Xunit.Assert;

namespace Hutch.Relay.Tests.Services;

public class RelaySubTaskServiceTests(Fixtures fixtures) : IClassFixture<Fixtures>, IAsyncLifetime
{
  private readonly ApplicationDbContext _dbContext = fixtures.DbContext;
  
  public async Task InitializeAsync()
  { }

  public async Task DisposeAsync()
  {
    // Clean up the database after each test
    _dbContext.RelayTasks.RemoveRange(_dbContext.RelayTasks);
    await _dbContext.SaveChangesAsync();
  }
  
  [Fact]
  public async Task Create_ValidRelaySubTaskModel_ReturnsCreatedRelaySubTaskModel()
  {
    // Arrange
    const string userId = "test-user-id";
    const string ownerId = "test-owner-id";
    var model = new RelaySubTaskModel
    {
      Id = "test-id",
      Owner = new SubNode
      {
        Id = ownerId,
        RelayUsers = new List<RelayUser>
        {
          new()
          {
            Id = userId,
            UserName = "testuser@example.com"
          }
        }
      }
    };

    var service = new RelaySubTaskService(_dbContext);

    // Act
    var result = await service.Create(model);

    // Assert
    Assert.NotNull(result);
    Assert.NotNull(result.Id);

    var entityInDb = await _dbContext.RelaySubTasks.FindAsync(result.Id);
    Assert.NotNull(entityInDb);
  }
}
