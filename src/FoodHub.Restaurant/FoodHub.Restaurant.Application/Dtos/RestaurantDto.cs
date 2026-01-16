using System;

namespace FoodHub.Restaurant.Application.Dtos;

public sealed record RestaurantDto(Guid Id, string Name, string City, bool IsActive);
