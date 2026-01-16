using FoodHub.Menu.Application.Dtos;
using FoodHub.Menu.Application.Interfaces;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace FoodHub.Menu.Application.Commands;

public record UpdateMenuItemDto(
    Guid MenuId,
    Guid MenuItemId,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    Domain.Enums.ItemCategory Category,
    IReadOnlyList<MenuImageDto>? Images = null
);

public class UpdateMenuItemCommand
{
    private readonly IMenuRepository _menuRepository;
    private readonly Serilog.ILogger _logger;

    public UpdateMenuItemCommand(IMenuRepository menuRepository, Serilog.ILogger logger)
    {
        _menuRepository = menuRepository;
        _logger = logger.ForContext<UpdateMenuItemCommand>();
    }

    public async Task ExecuteAsync(UpdateMenuItemDto dto, CancellationToken cancellationToken)
    {
        _logger.Information("Use Case: Updating item {MenuItemId} in Menu {MenuId}", dto.MenuItemId, dto.MenuId);

        var menu = await _menuRepository.GetByIdAsync(dto.MenuId, cancellationToken);
        if (menu is null)
        {
            throw new Exceptions.ApplicationException($"Menu with ID '{dto.MenuId}' not found.");
        }

        var price = new Domain.ValueObjects.Price(dto.Price, dto.Currency);

        IEnumerable<Domain.ValueObjects.MenuImage>? domainImages = null;
        if (dto.Images is not null)
            domainImages = dto.Images.Select(i => new Domain.ValueObjects.MenuImage(i.Type, i.Url));

        // The business logic for the update is handled by the domain entity itself
        menu.UpdateMenuItem(dto.MenuItemId, dto.Name, dto.Description, price, dto.Category, domainImages);

        await _menuRepository.UpdateAsync(menu, cancellationToken);

        _logger.Information("Successfully updated MenuItem {MenuItemId} in Menu {MenuId}", dto.MenuItemId, menu.Id);
    }
}
