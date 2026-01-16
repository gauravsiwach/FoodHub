namespace FoodHub.Menu.Application.Dtos;

public record MenuItemDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    string Category,
    string Availability,
    IReadOnlyList<MenuImageDto> Images
);

