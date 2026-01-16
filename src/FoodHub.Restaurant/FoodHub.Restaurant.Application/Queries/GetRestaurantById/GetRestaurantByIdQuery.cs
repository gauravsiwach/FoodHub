using System;
using System.Threading;
using System.Threading.Tasks;
using FoodHub.Restaurant.Application.Dtos;
using FoodHub.Restaurant.Application.Interfaces;

namespace FoodHub.Restaurant.Application.Queries.GetRestaurantById;

public sealed class GetRestaurantByIdQuery
{
    private readonly IRestaurantRepository _repository;

    public GetRestaurantByIdQuery(IRestaurantRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<RestaurantDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id must not be empty.", nameof(id));

        var restaurant = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (restaurant is null) return null;

        return new RestaurantDto(restaurant.Id, restaurant.Name.Value, restaurant.City, restaurant.IsActive);
    }
}
