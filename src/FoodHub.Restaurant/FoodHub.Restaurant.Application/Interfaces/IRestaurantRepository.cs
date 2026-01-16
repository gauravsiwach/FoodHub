using System;
using System.Threading;
using System.Threading.Tasks;
using FoodHub.Restaurant.Domain.Entities;
using RestaurantEntity = FoodHub.Restaurant.Domain.Entities.Restaurant;


namespace FoodHub.Restaurant.Application.Interfaces;


public interface IRestaurantRepository
{
    Task AddAsync(RestaurantEntity restaurant, CancellationToken cancellationToken = default);

    Task<RestaurantEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<RestaurantEntity>> GetAllAsync(CancellationToken cancellationToken = default);
}
