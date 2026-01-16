using FoodHub.Menu.Application.Interfaces;
using FoodHub.Restaurant.Infrastructure.Persistence.Cosmos;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using System;

namespace FoodHub.Restaurant.Infrastructure.Persistence.Repositories;

public class RestaurantReadRepository : IRestaurantReadRepository
{
    private readonly Container _container;

    public RestaurantReadRepository(CosmosContext context)
    {
        _container = context.Container;
    }

    public async Task<bool> ExistsAsync(Guid restaurantId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<RestaurantDocument>(
                restaurantId.ToString(),
                new PartitionKey(restaurantId.ToString()),
                cancellationToken: cancellationToken);
            
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}
