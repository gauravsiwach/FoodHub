using System;
using System.Threading;
using System.Threading.Tasks;
using FoodHub.User.Application.Dtos;
using FoodHub.User.Application.Interfaces;
using Serilog;

namespace FoodHub.User.Application.Commands.UpdateUser;

public sealed class UpdateUserCommand
{
    private readonly IUserRepository _repository;
    private readonly Serilog.ILogger _logger;

    public UpdateUserCommand(IUserRepository repository, Serilog.ILogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        _logger.Information("Use Case: Updating user {UserId}", dto.Id);

        var user = await _repository.GetByIdAsync(dto.Id, cancellationToken);
        if (user is null)
        {
            throw new Exceptions.ApplicationException($"User with ID '{dto.Id}' not found.");
        }

        user.UpdateProfile(dto.Name, dto.Phone);

        await _repository.UpdateAsync(user, cancellationToken);

        _logger.Information("Successfully updated user {UserId}", user.Id);
    }
}