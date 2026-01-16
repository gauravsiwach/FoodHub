using FoodHub.Restaurant.Application.Interfaces;
using Microsoft.Azure.Cosmos;
using RestaurantEntity = FoodHub.Restaurant.Domain.Entities.Restaurant;
using FoodHub.Restaurant.Infrastructure.Persistence.Cosmos;
using FoodHub.Restaurant.Domain.ValueObjects;
using System.Net;

namespace FoodHub.Restaurant.Infrastructure.Persistence.Repositories;

public sealed class RestaurantRepository : IRestaurantRepository
{
    private readonly Container _container;

    public RestaurantRepository(CosmosContext context)
    {
        _container = context.Container;
    }

    public async Task<IEnumerable<RestaurantEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = _container.GetItemQueryIterator<RestaurantDocument>(query);
        var results = new List<RestaurantEntity>();

        while (iterator.HasMoreResults)
        {
            var resp = await iterator.ReadNextAsync(cancellationToken);
            foreach (var doc in resp.Resource)
            {
                results.Add(new RestaurantEntity(
                    Guid.Parse(doc.Id),
                    new RestaurantName(doc.Name),
                    doc.City,
                    doc.IsActive));
            }
        }

        return results;
    }

    public async Task AddAsync(
        RestaurantEntity restaurant,
        CancellationToken cancellationToken = default)
    {
        var document = new RestaurantDocument
        {
            Id = restaurant.Id.ToString(),
            Name = restaurant.Name.Value,
            City = restaurant.City,
            IsActive = restaurant.IsActive
        };

        await _container.CreateItemAsync(
            document,
            new PartitionKey(document.Id),
            cancellationToken: cancellationToken);
    }


    public async Task<RestaurantEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<RestaurantDocument>(
                id.ToString(),
                new PartitionKey(id.ToString()),
                cancellationToken: cancellationToken);

            var doc = response.Resource;

            return new RestaurantEntity(
                Guid.Parse(doc.Id),
                new RestaurantName(doc.Name),
                doc.City,
                doc.IsActive);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
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
