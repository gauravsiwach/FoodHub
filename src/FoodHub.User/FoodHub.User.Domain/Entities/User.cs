using System;

namespace FoodHub.User.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public ValueObjects.Email Email { get; private set; }
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Private constructor for persistence mapping
    private User() { }

    // Primary constructor: creates a new user with a generated Id
    public User(string name, ValueObjects.Email email, string? phone = null)
        : this(Guid.NewGuid(), name, email, phone, true, DateTime.UtcNow)
    {
    }

    // Full constructor with validation (for rehydration from persistence)
    public User(Guid id, string name, ValueObjects.Email email, string? phone, bool isActive, DateTime createdAt)
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

    // Business operations
    public void UpdateProfile(string name, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.DomainException("User name cannot be empty.");

        Name = name.Trim();
        Phone = phone?.Trim();
    }

    public void UpdateEmail(ValueObjects.Email newEmail)
    {
        if (newEmail is null)
            throw new Exceptions.DomainException("Email is required.");

        Email = newEmail;
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }
}