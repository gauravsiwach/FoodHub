# Prompt 4: User Module Implementation

## Context
Implement the **User Module** with SQL Server database using Entity Framework Core. This module is standalone with no cross-module dependencies.

## Module Characteristics
- **Database:** SQL Server (local or Azure)
- **ORM:** Entity Framework Core 9.0.1
- **Pattern:** Code-First with migrations
- **Independence:** No dependencies on other modules

---

## Part 1: Domain Layer (FoodHub.User.Domain)

### 1.1 Domain Exception
```csharp
namespace FoodHub.User.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

### 1.2 Email Value Object (`ValueObjects/Email.cs`)
```csharp
using System.Text.RegularExpressions;
using FoodHub.User.Domain.Exceptions;

namespace FoodHub.User.Domain.ValueObjects;

public sealed record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty.");

        var normalized = value.Trim().ToLowerInvariant();
        if (!EmailRegex.IsMatch(normalized))
            throw new DomainException($"Invalid email format: {value}");

        Value = normalized;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
```

### 1.3 User Entity (`Entities/User.cs`)
```csharp
using FoodHub.User.Domain.ValueObjects;

namespace FoodHub.User.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User() { }

    public User(string name, Email email, string? phone = null)
        : this(Guid.NewGuid(), name, email, phone, true, DateTime.UtcNow)
    {
    }

    public User(Guid id, string name, Email email, string? phone, bool isActive, DateTime createdAt)
    {
        if (id == Guid.Empty)
            throw new Exceptions.DomainException("User Id must not be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.DomainException("User name cannot be empty.");
        if (email is null)
            throw new Exceptions.DomainException("Email is required.");

        Id = id;
        Name = name.Trim();
        Email = email;
        Phone = phone?.Trim();
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    public void UpdateProfile(string name, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.DomainException("User name cannot be empty.");
        Name = name.Trim();
        Phone = phone?.Trim();
    }

    public void UpdateEmail(Email newEmail)
    {
        if (newEmail is null)
            throw new Exceptions.DomainException("Email is required.");
        Email = newEmail;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
```

---

## Part 2: Application Layer (FoodHub.User.Application)

### 2.1 Repository Interface
```csharp
using UserEntity = FoodHub.User.Domain.Entities.User;

namespace FoodHub.User.Application.Interfaces;

public interface IUserRepository
{
    Task AddAsync(UserEntity user, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserEntity user, CancellationToken cancellationToken = default);
}
```

### 2.2 DTOs
```csharp
namespace FoodHub.User.Application.Dtos;

public record UserDto(Guid Id, string Name, string Email, string? Phone, bool IsActive, DateTime CreatedAt);
public record CreateUserDto(string Name, string Email, string? Phone);
```

### 2.3 Commands & Queries
Implement similar to Restaurant module:
- `CreateUserCommand`
- `GetUserByIdQuery`
- `GetUserByEmailQuery`
- `GetAllUserQuery`

---

## Part 3: Infrastructure Layer (FoodHub.User.Infrastructure)

### 3.1 Package Dependencies
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.1" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.2" />
```

### 3.2 EF Core Entity (`Sql/Models/UserEntity.cs`)
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodHub.User.Infrastructure.Sql.Models;

[Table("Users")]
public sealed class UserEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [Required]
    [MaxLength(320)]
    public string Email { get; set; } = default!;

    [MaxLength(50)]
    public string? Phone { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserEntity() { }

    public Domain.Entities.User ToDomain()
    {
        var domainEmail = new Domain.ValueObjects.Email(Email);
        return new Domain.Entities.User(Id, Name, domainEmail, Phone, IsActive, CreatedAt);
    }

    public static UserEntity FromDomain(Domain.Entities.User user)
    {
        return new UserEntity
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email.Value,
            Phone = user.Phone,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
```

### 3.3 DbContext (`Sql/UserDbContext.cs`)
```csharp
using FoodHub.User.Infrastructure.Sql.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodHub.User.Infrastructure.Sql;

public sealed class UserDbContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; } = default!;

    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(320);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("IX_Users_Email");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_Users_IsActive");
        });
    }
}
```

### 3.4 Repository Implementation
```csharp
using FoodHub.User.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserEntity = FoodHub.User.Domain.Entities.User;

namespace FoodHub.User.Infrastructure.Sql.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        var userEntity = Models.UserEntity.FromDomain(user);
        _context.Users.Add(userEntity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userEntity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        return userEntity?.ToDomain();
    }

    public async Task<List<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var userEntities = await _context.Users.AsNoTracking().ToListAsync(cancellationToken);
        return userEntities.Select(u => u.ToDomain()).ToList();
    }

    public async Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var userEntity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        return userEntity?.ToDomain();
    }

    public async Task UpdateAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        var existingEntity = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);
        if (existingEntity is null)
            throw new InvalidOperationException($"User with ID {user.Id} not found for update.");

        existingEntity.Name = user.Name;
        existingEntity.Phone = user.Phone;
        existingEntity.IsActive = user.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

### 3.5 Dependency Injection (`DependencyInjection.cs`)
```csharp
using FoodHub.User.Application.Interfaces;
using FoodHub.User.Infrastructure.Sql;
using FoodHub.User.Infrastructure.Sql.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodHub.User.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUserModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(UserDbContext).Assembly.FullName)));

        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}
```

---

## Expected Output
1. Complete User module with Domain, Application, Infrastructure
2. Email value object with validation
3. SQL Server integration with Entity Framework Core
4. DbContext with proper entity configuration
5. Repository pattern with EF Core
6. Dependency injection module registration

## Success Criteria
- User module builds successfully
- EF Core migrations can be created
- Database schema properly defined
- Email validation works
- Repository implements all CRUD operations