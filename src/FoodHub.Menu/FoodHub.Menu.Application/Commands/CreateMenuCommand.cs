using FoodHub.Menu.Application.Interfaces;
using FoodHub.Menu.Application.Dtos;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace FoodHub.Menu.Application.Commands;

public record CreateMenuItemDto(string Name, string Description, decimal Price, string Currency, Domain.Enums.ItemCategory Category, IReadOnlyList<MenuImageDto>? Images = null);

public record CreateMenuDto(Guid RestaurantId, string Name, string? Description = null, IReadOnlyList<CreateMenuItemDto>? Items = null);

public class CreateMenuCommand
{
    private readonly IMenuRepository _menuRepository;
    private readonly IRestaurantReadRepository _restaurantReadRepository;
    private readonly Serilog.ILogger _logger;

    public CreateMenuCommand(IMenuRepository menuRepository, IRestaurantReadRepository restaurantReadRepository, Serilog.ILogger logger)
    {
        _menuRepository = menuRepository;
        _restaurantReadRepository = restaurantReadRepository;
        _logger = logger.ForContext<CreateMenuCommand>();
    }

    public async Task<Guid> ExecuteAsync(CreateMenuDto dto, CancellationToken cancellationToken)
    {
        _logger.Information("Use Case: Creating menu for Restaurant {RestaurantId}", dto.RestaurantId);

        var restaurantExists = await _restaurantReadRepository.ExistsAsync(dto.RestaurantId, cancellationToken);
        if (!restaurantExists)
        {
            throw new Exceptions.ApplicationException($"Restaurant with ID '{dto.RestaurantId}' does not exist.");
        }

        var menu = Domain.Entities.Menu.Create(dto.RestaurantId, dto.Name, dto.Description);

        // Add initial menu items if provided
        if (dto.Items is not null && dto.Items.Count > 0)
        {
            foreach (var it in dto.Items)
            {
                var price = new Domain.ValueObjects.Price(it.Price, it.Currency);
                IEnumerable<Domain.ValueObjects.MenuImage>? domainImages = null;
                if (it.Images is not null)
                    domainImages = it.Images.Select(i => new Domain.ValueObjects.MenuImage(i.Type, i.Url));

                var menuItem = Domain.Entities.MenuItem.Create(it.Name, it.Description, price, it.Category, domainImages);
                menu.AddMenuItem(menuItem);
            }
        }

        await _menuRepository.AddAsync(menu, cancellationToken);

        _logger.Information("Successfully created Menu {MenuId} for Restaurant {RestaurantId}", menu.Id, menu.RestaurantId);
        
        return menu.Id;
    }
}
