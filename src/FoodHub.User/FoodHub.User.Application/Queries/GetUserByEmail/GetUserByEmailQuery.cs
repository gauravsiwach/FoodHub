using System;
using System.Threading;
using System.Threading.Tasks;
using FoodHub.User.Application.Dtos;
using FoodHub.User.Application.Interfaces;
using Serilog;

namespace FoodHub.User.Application.Queries.GetUserByEmail;

public sealed class GetUserByEmailQuery
{
    private readonly IUserRepository _repository;
    private readonly Serilog.ILogger _logger;

    public GetUserByEmailQuery(IUserRepository repository, Serilog.ILogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserDto?> ExecuteAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email must not be empty.", nameof(email));

        _logger.Information("Use Case: Getting user by email {Email}", email);

        var user = await _repository.GetByEmailAsync(email, cancellationToken);
        if (user == null)
        {
            _logger.Information("User with email {Email} not found", email);
            return null;
        }

        _logger.Information("Successfully retrieved user {UserId} with email {Email}", user.Id, user.Email.Value);

        return new UserDto(user.Id, user.Name, user.Email.Value, user.Phone, user.IsActive, user.CreatedAt);
    }
}