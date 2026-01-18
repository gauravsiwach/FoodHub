using System;
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
    [MaxLength(320)]  // RFC 5321 maximum email length
    public string Email { get; set; } = default!;

    [MaxLength(50)]
    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    // Parameterless constructor for EF Core
    public UserEntity() { }

    // Constructor for mapping from Domain
    public UserEntity(Guid id, string name, string email, string? phone, bool isActive, DateTime createdAt)
    {
        Id = id;
        Name = name;
        Email = email;
        Phone = phone;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    // Convert to Domain entity
    public Domain.Entities.User ToDomain()
    {
        var domainEmail = new Domain.ValueObjects.Email(Email);
        return new Domain.Entities.User(Id, Name, domainEmail, Phone, IsActive, CreatedAt);
    }

    // Create from Domain entity
    public static UserEntity FromDomain(Domain.Entities.User user)
    {
        return new UserEntity(user.Id, user.Name, user.Email.Value, user.Phone, user.IsActive, user.CreatedAt);
    }
}