using FoodHub.Menu.Application.Dtos;
using FoodHub.Menu.Application.Interfaces;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace FoodHub.Menu.Application.Commands;

public record AddMenuItemDto(Guid MenuId, string Name, string Description, decimal Price, string Currency, Domain.Enums.ItemCategory Category, IReadOnlyList<MenuImageDto>? Images = null);

public class AddMenuItemCommand
{
    private readonly IMenuRepository _menuRepository;
    private readonly Serilog.ILogger _logger;

    public AddMenuItemCommand(IMenuRepository menuRepository, Serilog.ILogger logger)
    {
        _menuRepository = menuRepository;
        _logger = logger.ForContext<AddMenuItemCommand>();
    }

    public async Task ExecuteAsync(AddMenuItemDto dto, CancellationToken cancellationToken)
    {
        _logger.Information("Use Case: Adding menu item to Menu {MenuId}", dto.MenuId);

        var menu = await _menuRepository.GetByIdAsync(dto.MenuId, cancellationToken);
        if (menu is null)
        {
            throw new Exceptions.ApplicationException($"Menu with ID '{dto.MenuId}' not found.");
        }

        var price = new Domain.ValueObjects.Price(dto.Price, dto.Currency);
        IEnumerable<Domain.ValueObjects.MenuImage>? domainImages = null;
        if (dto.Images is not null)
            domainImages = dto.Images.Select(i => new Domain.ValueObjects.MenuImage(i.Type, i.Url));

        var menuItem = Domain.Entities.MenuItem.Create(dto.Name, dto.Description, price, dto.Category, domainImages);

        menu.AddMenuItem(menuItem);

        await _menuRepository.UpdateAsync(menu, cancellationToken);

        _logger.Information("Successfully added MenuItem {MenuItemId} to Menu {MenuId}", menuItem.Id, menu.Id);
    }
}
