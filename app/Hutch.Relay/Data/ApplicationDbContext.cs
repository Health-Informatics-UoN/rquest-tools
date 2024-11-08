using Hutch.Relay.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Task = Hutch.Relay.Data.Entities.Task;

namespace Hutch.Relay.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
  : IdentityDbContext<IdentityUser>(options)
{
  public DbSet<RelayUser> RelayUsers { get; set; }
  public DbSet<SubNode> SubNodes { get; set; }
  public DbSet<Task> Tasks { get; set; }
  public DbSet<SubTask> SubTasks { get; set; }
  
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Task>()
      .Property(t => t.CreatedAt)
      .HasDefaultValueSql("CURRENT_TIMESTAMP");
  }
}
