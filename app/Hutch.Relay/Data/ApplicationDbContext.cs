using Hutch.Relay.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hutch.Relay.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
  : IdentityDbContext<IdentityUser>(options)
{
  public DbSet<RelayUser> RelayUsers { get; set; }
  public DbSet<SubNode> SubNodes { get; set; }
}
