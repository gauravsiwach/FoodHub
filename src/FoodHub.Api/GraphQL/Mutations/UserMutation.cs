using FoodHub.User.Application.Commands.CreateUser;
using FoodHub.User.Application.Commands.UpdateUser;
using FoodHub.User.Application.Queries.GetUserById;
using FoodHub.User.Application.Dtos;
using FoodHub.User.Application.Interfaces;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FoodHub.Api.GraphQL.Mutations;

[Authorize]
[ExtendObjectType("Mutation")]
public sealed class UserMutation
{
    public async Task<Guid> CreateUser(
        CreateUserDto input,
        [Service] IUserRepository repository,
        [Service] Serilog.ILogger logger,
        [Service] Serilog.ILogger commandLogger,
        CancellationToken cancellationToken)
    {
        logger.ForContext<UserMutation>().Information("Begin: CreateUser mutation for {Name}, {Email}", input.Name, input.Email);

        try
        {
            var command = new CreateUserCommand(repository, commandLogger.ForContext<CreateUserCommand>());
            var userId = await command.ExecuteAsync(input, cancellationToken);
            
            logger.ForContext<UserMutation>().Information(
                "Success: Created user {Name} with Id {UserId}", 
                input.Name, 
                userId);
            
            return userId;
        }
        catch (Exception ex)
        {
            logger.ForContext<UserMutation>().Error(ex, "Error: Failed to create user {Name}", input.Name);
            throw;
        }
    }

    public async Task<UserDto?> UpdateUser(
        UpdateUserDto input,
        [Service] IUserRepository repository,
        [Service] Serilog.ILogger logger,
        [Service] Serilog.ILogger commandLogger,
        CancellationToken cancellationToken)
    {
        logger.ForContext<UserMutation>().Information("Begin: UpdateUser mutation for {UserId}", input.Id);

        try
        {
            var command = new UpdateUserCommand(repository, commandLogger.ForContext<UpdateUserCommand>());
            await command.ExecuteAsync(input, cancellationToken);
            
            // Get updated user to return
            var query = new GetUserByIdQuery(repository, commandLogger.ForContext<GetUserByIdQuery>());
            var updatedUser = await query.ExecuteAsync(input.Id, cancellationToken);
            
            if (updatedUser != null)
            {
                logger.ForContext<UserMutation>().Information("Success: Updated user {UserId}", input.Id);
            }
            else
            {
                logger.ForContext<UserMutation>().Information("NotFound: User {UserId} not found for update", input.Id);
            }
            
            return updatedUser;
        }
        catch (Exception ex)
        {
            logger.ForContext<UserMutation>().Error(ex, "Error: Failed to update user {UserId}", input.Id);
            throw;
        }
    }
}