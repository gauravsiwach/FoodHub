using FoodHub.Menu.Application.Dtos;
using FoodHub.Menu.Application.Interfaces;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodHub.Menu.Application.Dtos;

namespace FoodHub.Menu.Application.Queries;

public class GetMenuByIdQuery
{
    private readonly IMenuRepository _menuRepository;
    private readonly Serilog.ILogger _logger;

    public GetMenuByIdQuery(IMenuRepository menuRepository, Serilog.ILogger logger)
    {
        _menuRepository = menuRepository;
        _logger = logger.ForContext<GetMenuByIdQuery>();
    }

    public async Task<MenuDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.Information("Use Case: Getting menu by ID {MenuId}", id);

        var menu = await _menuRepository.GetByIdAsync(id, cancellationToken);
        if (menu is null)
        {
            _logger.Warning("Menu with ID {MenuId} not found", id);
            return null;
        }

        // Map item images and DTOs
        // Compute price range if there are items
        MenuPriceDto? priceRange = null;
        if (menu.MenuItems.Any())
        {
            var firstCurrency = menu.MenuItems.First().Price.Currency;
            var amounts = menu.MenuItems.Select(mi => mi.Price.Amount).ToList();
            priceRange = new MenuPriceDto(amounts.Min(), amounts.Max(), firstCurrency);
        }
        // Build MenuItem DTOs with their own images
        var menuItemDtos = menu.MenuItems
            .Select(mi => new MenuItemDto(mi.Id, mi.Name, mi.Description, mi.Price.Amount, mi.Price.Currency, mi.Category.ToString(), mi.Availability.ToString(), mi.Images.Select(i => new MenuImageDto(i.Type, i.Url)).ToList()))
            .ToList();

        return new MenuDto(menu.Id, menu.RestaurantId, menu.Name, menu.Description, menuItemDtos, priceRange);
    }
}
