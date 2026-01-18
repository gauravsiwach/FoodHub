using System;
using System.Threading;
using System.Threading.Tasks;
using FoodHub.User.Application.Dtos;
using FoodHub.User.Application.Interfaces;
using FoodHub.User.Domain.Entities;
using FoodHub.User.Domain.ValueObjects;
using Serilog;

namespace FoodHub.User.Application.Commands.CreateUser;

public sealed class CreateUserCommand
{
    private readonly IUserRepository _repository;
    private readonly Serilog.ILogger _logger;

    public CreateUserCommand(IUserRepository repository, Serilog.ILogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> ExecuteAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        _logger.Information("Use Case: Creating user with email {Email}", dto.Email);

        // Check if email already exists
        var existingUser = await _repository.GetByEmailAsync(dto.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new Exceptions.ApplicationException($"User with email '{dto.Email}' already exists.");
        }

        var email = new Email(dto.Email);
        var user = new Domain.Entities.User(dto.Name, email, dto.Phone);

        await _repository.AddAsync(user, cancellationToken);

        _logger.Information("Successfully created user {UserId} with email {Email}", user.Id, user.Email.Value);
        
        return user.Id;
    }
}