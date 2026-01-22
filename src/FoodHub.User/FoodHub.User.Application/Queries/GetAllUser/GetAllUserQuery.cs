using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodHub.User.Application.Dtos;
using FoodHub.User.Application.Interfaces;
using Serilog;

namespace FoodHub.User.Application.Queries.GetAllUser;

public sealed class GetAllUserQuery
{
    private readonly IUserRepository _repository;
    private readonly Serilog.ILogger _logger;

    public GetAllUserQuery(IUserRepository repository, Serilog.ILogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<UserDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.Information("Use Case: Getting all users");

        var users = await _repository.GetAllAsync(cancellationToken);
        
        _logger.Information("Successfully retrieved {UserCount} users", users.Count);

        return users.Select(user => new UserDto(
            user.Id, 
            user.Name, 
            user.Email.Value, 
            user.Phone, 
            user.IsActive, 
            user.CreatedAt))
            .ToList();
    }
}