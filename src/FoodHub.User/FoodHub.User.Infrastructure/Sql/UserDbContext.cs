using FoodHub.User.Infrastructure.Sql.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodHub.User.Infrastructure.Sql;

public sealed class UserDbContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; } = default!;

    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(320);

            entity.Property(e => e.Phone)
                .HasMaxLength(50);

            entity.Property(e => e.IsActive)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            // Create unique index on Email
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            // Create index on IsActive for filtering
            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Users_IsActive");
        });
    }
}