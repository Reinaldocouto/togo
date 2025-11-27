using Microsoft.EntityFrameworkCore;
using Togo.Domain.Entities;

namespace Togo.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var user = modelBuilder.Entity<User>();
        user.ToTable("Users");
        user.HasKey(u => u.Id);
        user.Property(u => u.Id).ValueGeneratedNever();
        user.Property(u => u.Name).IsRequired().HasMaxLength(200);
        user.Property(u => u.Email).IsRequired().HasMaxLength(320);
        user.HasIndex(u => u.Email).IsUnique();
        user.Property(u => u.PasswordHash).IsRequired().HasMaxLength(200);
    }
}
