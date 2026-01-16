using System;
using System.Threading;
using System.Threading.Tasks;
using FoodHub.Restaurant.Application.Dtos;
using FoodHub.Restaurant.Application.Interfaces;
using FoodHub.Restaurant.Domain.Entities;
using FoodHub.Restaurant.Domain.ValueObjects;



namespace FoodHub.Restaurant.Application.Commands.CreateRestaurant;

public sealed class CreateRestaurantCommand
{
    private readonly IRestaurantRepository _repository;

    public CreateRestaurantCommand(IRestaurantRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Guid> ExecuteAsync(CreateRestaurantDto dto, CancellationToken cancellationToken = default)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        var name = new RestaurantName(dto.Name);
            var restaurant = new FoodHub.Restaurant.Domain.Entities.Restaurant(name, dto.City); // Ensure Restaurant is a class in the correct namespace

        await _repository.AddAsync(restaurant, cancellationToken).ConfigureAwait(false);

        return restaurant.Id;
    }
}
