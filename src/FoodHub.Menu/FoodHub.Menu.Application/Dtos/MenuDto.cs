using System.Collections.Generic;

namespace FoodHub.Menu.Application.Dtos;

public record MenuDto(
    Guid Id,
    Guid RestaurantId,
    string Name,
    string? Description,
    IReadOnlyList<MenuItemDto> MenuItems,
    MenuPriceDto? PriceRange
);
