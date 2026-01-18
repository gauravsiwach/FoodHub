using System;

namespace FoodHub.User.Application.Dtos;

public sealed record UserDto(Guid Id, string Name, string Email, string? Phone, bool IsActive, DateTime CreatedAt);

public sealed record CreateUserDto(string Name, string Email, string? Phone = null);

public sealed record UpdateUserDto(Guid Id, string Name, string? Phone);