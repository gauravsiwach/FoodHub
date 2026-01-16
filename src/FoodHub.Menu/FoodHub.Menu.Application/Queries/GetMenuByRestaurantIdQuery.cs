using FoodHub.Menu.Application.Dtos;
using FoodHub.Menu.Application.Interfaces;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoodHub.Menu.Application.Queries;

public class GetMenuByRestaurantIdQuery
{
    private readonly IMenuRepository _menuRepository;
    private readonly Serilog.ILogger _logger;

    public GetMenuByRestaurantIdQuery(IMenuRepository menuRepository, Serilog.ILogger logger)
    {
        _menuRepository = menuRepository;
        _logger = logger.ForContext<GetMenuByRestaurantIdQuery>();
    }

    public async Task<MenuDto?> ExecuteAsync(Guid restaurantId, CancellationToken cancellationToken)
    {
        _logger.Information("Use Case: Getting menu for Restaurant {RestaurantId}", restaurantId);

        var menu = await _menuRepository.GetByRestaurantIdAsync(restaurantId, cancellationToken);
        if (menu is null)
        {
            _logger.Warning("Menu for Restaurant {RestaurantId} not found", restaurantId);
            return null;
        }

        MenuPriceDto? priceRange = null;
        if (menu.MenuItems.Any())
        {
            var firstCurrency = menu.MenuItems.First().Price.Currency;
            var amounts = menu.MenuItems.Select(mi => mi.Price.Amount).ToList();
            priceRange = new MenuPriceDto(amounts.Min(), amounts.Max(), firstCurrency);
        }

        var menuItemDtos = menu.MenuItems
            .Select(mi => new MenuItemDto(mi.Id, mi.Name, mi.Description, mi.Price.Amount, mi.Price.Currency, mi.Category.ToString(), mi.Availability.ToString(), mi.Images.Select(i => new MenuImageDto(i.Type, i.Url)).ToList()))
            .ToList();

        return new MenuDto(menu.Id, menu.RestaurantId, menu.Name, menu.Description, menuItemDtos, priceRange);
    }
}
