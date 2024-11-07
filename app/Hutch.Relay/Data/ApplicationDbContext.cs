using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hutch.Relay.Data;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
  : IdentityDbContext<IdentityUser>(options);
