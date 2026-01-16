using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodHub.Restaurant.Application.Dtos;
using FoodHub.Restaurant.Application.Interfaces;

namespace FoodHub.Restaurant.Application.Queries.GetAllRestaurants;

public sealed class GetAllRestaurantsQuery
{
    private readonly IRestaurantRepository _repository;

    public GetAllRestaurantsQuery(IRestaurantRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<RestaurantDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var restaurants = await _repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return restaurants.Select(r => new RestaurantDto(r.Id, r.Name.Value, r.City, r.IsActive)).ToList();
    }
}
