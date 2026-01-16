using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;

using FoodHub.Menu.Domain.Entities;
using MenuEntity = FoodHub.Menu.Domain.Entities.Menu;
using FoodHub.Menu.Application.Interfaces;

namespace FoodHub.Menu.Infrastructure.Persistence.Cosmos;

public class MenuRepository : IMenuRepository
{
    private readonly Container _container;
    private readonly Serilog.ILogger _logger;

    public MenuRepository(CosmosContext context, Serilog.ILogger logger)
    {
        _logger = logger?.ForContext<MenuRepository>() ?? throw new ArgumentNullException(nameof(logger));
        if (context is null) throw new ArgumentNullException(nameof(context));

        _container = context.GetContainer("Menus");
    }

    public async Task<MenuEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.Debug("Fetching Menu by id {MenuId} from Cosmos", id);

        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id").WithParameter("@id", id);
        var iterator = _container.GetItemQueryIterator<MenuDocument>(query);
        while (iterator.HasMoreResults)
        {
            var resp = await iterator.ReadNextAsync(cancellationToken);
            var doc = resp.Resource.FirstOrDefault();
            if (doc != null)
            {
                return doc.ToDomain();
            }
        }

        return null;
    }

    public async Task<MenuEntity?> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken)
    {
        _logger.Debug("Fetching Menu for Restaurant {RestaurantId} from Cosmos", restaurantId);

        var query = new QueryDefinition("SELECT * FROM c WHERE c.restaurantId = @rid").WithParameter("@rid", restaurantId);
        var iterator = _container.GetItemQueryIterator<MenuDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(restaurantId.ToString()) });
        while (iterator.HasMoreResults)
        {
            var resp = await iterator.ReadNextAsync(cancellationToken);
            var doc = resp.Resource.FirstOrDefault();
            if (doc != null)
            {
                return doc.ToDomain();
            }
        }

        return null;
    }

    public async Task AddAsync(MenuEntity menu, CancellationToken cancellationToken)
    {
        _logger.Information("Inserting Menu {MenuId} into Cosmos", menu.Id);
        var doc = MenuDocument.FromDomain(menu);
        await _container.CreateItemAsync(doc, new PartitionKey(menu.RestaurantId.ToString()), cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(MenuEntity menu, CancellationToken cancellationToken)
    {
        _logger.Information("Upserting Menu {MenuId} into Cosmos", menu.Id);
        var doc = MenuDocument.FromDomain(menu);
        await _container.UpsertItemAsync(doc, new PartitionKey(menu.RestaurantId.ToString()), cancellationToken: cancellationToken);
    }
}
