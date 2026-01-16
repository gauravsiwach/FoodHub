using FoodHub.Restaurant.Application.Commands.CreateRestaurant;
using FoodHub.Restaurant.Application.Dtos;
using FoodHub.Restaurant.Application.Interfaces;
using FoodHub.Menu.Application.Commands;
using FoodHub.Menu.Application.Dtos;
using FoodHub.Menu.Application.Interfaces;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FoodHub.Api.GraphQL.Mutations;

public sealed class RestaurantMutation
{
    public async Task<Guid> CreateRestaurant(
        CreateRestaurantDto input,
        [Service] IRestaurantRepository repository,
        [Service] Serilog.ILogger logger,
        CancellationToken cancellationToken)
    {
        logger.ForContext<RestaurantMutation>().Information("Begin: CreateRestaurant mutation for {RestaurantName}, {City}", input.Name, input.City);

        try
        {
            var command = new CreateRestaurantCommand(repository);
            var restaurantId = await command.ExecuteAsync(input, cancellationToken);
            
            logger.ForContext<RestaurantMutation>().Information(
                "Success: Created restaurant {RestaurantName} with Id {RestaurantId}", 
                input.Name, 
                restaurantId);
            
            return restaurantId;
        }
        catch (Exception ex)
        {
            logger.ForContext<RestaurantMutation>().Error(ex, "Error: Failed to create restaurant {RestaurantName}", input.Name);
            throw;
        }
    }

    public async Task<Guid> CreateMenu(
        CreateMenuDto input,
        [Service] IMenuRepository repository,
        [Service] FoodHub.Menu.Application.Interfaces.IRestaurantReadRepository restaurantReadRepository,
        [Service] Serilog.ILogger logger,
        [Service] Serilog.ILogger commandLogger,
        CancellationToken cancellationToken)
    {
        logger.ForContext<RestaurantMutation>().Information("Begin: CreateMenu mutation for Restaurant {RestaurantId}, {Name}", input.RestaurantId, input.Name);

        try
        {
            var command = new CreateMenuCommand(repository, restaurantReadRepository, commandLogger.ForContext<CreateMenuCommand>());
            var createdId = await command.ExecuteAsync(input, cancellationToken);

            logger.ForContext<RestaurantMutation>().Information("Success: Created Menu {MenuId} for Restaurant {RestaurantId}", createdId, input.RestaurantId);

            return createdId;
        }
        catch (Exception ex)
        {
            logger.ForContext<RestaurantMutation>().Error(ex, "Error: Failed to create menu for Restaurant {RestaurantId}", input.RestaurantId);
            throw;
        }
    }

    public async Task AddMenuItem(
        AddMenuItemDto input,
        [Service] IMenuRepository repository,
        [Service] Serilog.ILogger logger,
        [Service] Serilog.ILogger commandLogger,
        CancellationToken cancellationToken)
    {
        logger.ForContext<RestaurantMutation>().Information("Begin: AddMenuItem mutation for Menu {MenuId}, {Name}", input.MenuId, input.Name);

        try
        {
            var command = new AddMenuItemCommand(repository, commandLogger.ForContext<AddMenuItemCommand>());
            await command.ExecuteAsync(input, cancellationToken);

            logger.ForContext<RestaurantMutation>().Information("Success: Added MenuItem to Menu {MenuId}", input.MenuId);

            return;
        }
        catch (Exception ex)
        {
            logger.ForContext<RestaurantMutation>().Error(ex, "Error: Failed to add menu item to Menu {MenuId}", input.MenuId);
            throw;
        }
    }

    public async Task UpdateMenuItem(
        UpdateMenuItemDto input,
        [Service] IMenuRepository repository,
        [Service] Serilog.ILogger logger,
        [Service] Serilog.ILogger commandLogger,
        CancellationToken cancellationToken)
    {
        logger.ForContext<RestaurantMutation>().Information("Begin: UpdateMenuItem mutation for Menu {MenuId}, Item {MenuItemId}", input.MenuId, input.MenuItemId);

        try
        {
            var command = new UpdateMenuItemCommand(repository, commandLogger.ForContext<UpdateMenuItemCommand>());
            await command.ExecuteAsync(input, cancellationToken);

            logger.ForContext<RestaurantMutation>().Information("Success: Updated MenuItem {MenuItemId} in Menu {MenuId}", input.MenuItemId, input.MenuId);

            return;
        }
        catch (Exception ex)
        {
            logger.ForContext<RestaurantMutation>().Error(ex, "Error: Failed to update menu item {MenuItemId} in Menu {MenuId}", input.MenuItemId, input.MenuId);
            throw;
        }
    }
}