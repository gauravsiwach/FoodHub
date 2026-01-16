using System;

namespace FoodHub.Restaurant.Domain.ValueObjects;

public sealed record RestaurantName
{
    public string Value { get; }

    public RestaurantName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new Exceptions.DomainException("Restaurant name must not be empty.");

        Value = value.Trim();
    }

    public override string ToString() => Value;

    public static implicit operator string(RestaurantName name) => name.Value;
    public static explicit operator RestaurantName(string value) => new RestaurantName(value);
}
