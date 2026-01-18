using System;
using System.Threading;
using System.Threading.Tasks;
using FoodHub.User.Application.Dtos;
using FoodHub.User.Application.Interfaces;
using Serilog;

namespace FoodHub.User.Application.Queries.GetUserById;

public sealed class GetUserByIdQuery
{
    private readonly IUserRepository _repository;
    private readonly Serilog.ILogger _logger;

    public GetUserByIdQuery(IUserRepository repository, Serilog.ILogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id must not be empty.", nameof(id));

        _logger.Information("Use Case: Getting user by ID {UserId}", id);

        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            _logger.Information("User with ID {UserId} not found", id);
            return null;
        }

        _logger.Information("Successfully retrieved user {UserId}", user.Id);

        return new UserDto(user.Id, user.Name, user.Email.Value, user.Phone, user.IsActive, user.CreatedAt);
    }
}