using System;

namespace FoodHub.Restaurant.Domain.Entities;

public class Restaurant
{
    public Guid Id { get; private set; }

    public ValueObjects.RestaurantName Name { get; private set; }

    public string City { get; private set; }

    public bool IsActive { get; private set; }

    // Primary constructor: creates a new restaurant with a generated Id

    public Restaurant(ValueObjects.RestaurantName name, string city)
        : this(Guid.NewGuid(), name, city, true)
    {
    }

    // Full constructor with validation
    public Restaurant(Guid id, ValueObjects.RestaurantName name, string city, bool isActive)
    {
        if (id == Guid.Empty)
            throw new Exceptions.DomainException("Id must not be empty.");

        if (name is null)
            throw new Exceptions.DomainException("Name is required.");

        if (string.IsNullOrWhiteSpace(city))
            throw new Exceptions.DomainException("City is required.");

        Id = id;
        Name = name;
        City = city.Trim();
        IsActive = isActive;
    }

    // Business operation: deactivate the restaurant
    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }

    // Optional convenience operations
    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
    }

    public void ChangeName(ValueObjects.RestaurantName newName)
    {
        if (newName is null)
            throw new Exceptions.DomainException("Name is required.");

        Name = newName;
    }
}
