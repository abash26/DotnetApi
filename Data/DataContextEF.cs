using DotnetApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetApi.Data;

public class DataContextEF : DbContext
{
  private readonly IConfiguration _config;

  public DataContextEF(IConfiguration config)
  {
    _config = config;
  }

  public virtual DbSet<User> Users { get; set; }
  public virtual DbSet<UserSalary> UserSalary { get; set; }
  public virtual DbSet<UserJobInfo> UserJobInfo { get; set; }
  public DbSet<Auth> Auth { get; set; }
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (!optionsBuilder.IsConfigured)
    {
      optionsBuilder.UseSqlServer(_config.GetConnectionString("DefaultConnection"),
        optionsBuilder => optionsBuilder.EnableRetryOnFailure());
    }
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema("TutorialAppSchema");

    modelBuilder.Entity<User>()
      .ToTable("Users", "TutorialAppSchema")
      .HasKey(user => user.UserId);

    modelBuilder.Entity<UserSalary>()
      .HasKey(user => user.UserId);

    modelBuilder.Entity<UserJobInfo>()
      .HasKey(user => user.UserId);
  }
}