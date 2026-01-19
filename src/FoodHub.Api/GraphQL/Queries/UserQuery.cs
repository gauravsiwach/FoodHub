using FoodHub.User.Application.Dtos;
using FoodHub.User.Application.Interfaces;
using FoodHub.User.Application.Queries.GetAllUser;
using FoodHub.User.Application.Queries.GetUserById;
using FoodHub.User.Application.Queries.GetUserByEmail;
using HotChocolate;
using HotChocolate.Types;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FoodHub.Api.GraphQL.Queries;

[ExtendObjectType("Query")]
public sealed class UserQuery
{
    public async Task<UserDto?> GetUserById(
        Guid id,
        [Service] IUserRepository repository,
        [Service] Serilog.ILogger logger,
        [Service] Serilog.ILogger queryLogger,
        CancellationToken cancellationToken)
    {
        logger.ForContext<UserQuery>().Information("Begin: GetUserById query for {UserId}", id);

        try
        {
            var query = new GetUserByIdQuery(repository, queryLogger.ForContext<GetUserByIdQuery>());
            var user = await query.ExecuteAsync(id, cancellationToken);

            if (user != null)
            {
                logger.ForContext<UserQuery>().Information("Success: Retrieved user {UserId}", id);
            }
            else
            {
                logger.ForContext<UserQuery>().Information("NotFound: User {UserId} not found", id);
            }

            return user;
        }
        catch (Exception ex)
        {
            logger.ForContext<UserQuery>().Error(ex, "Error: Failed to get user by id {UserId}", id);
            throw;
        }
    }

    public async Task<List<UserDto>> GetAllUsers(
        [Service] IUserRepository repository,
        [Service] Serilog.ILogger logger,
        [Service] Serilog.ILogger queryLogger,
        CancellationToken cancellationToken)
    {
        logger.ForContext<UserQuery>().Information("Begin: GetAllUsers query");
        try
        {
            var query = new GetAllUserQuery(repository, queryLogger.ForContext<GetAllUserQuery>());
            var users = await query.ExecuteAsync(cancellationToken);
            logger.ForContext<UserQuery>().Information("Success: Retrieved all users, count: {UserCount}", users.Count);
            return users;
        }
        catch (Exception ex)
        {
            logger.ForContext<UserQuery>().Error(ex, "Error: Failed to get all users");
            throw;
        }
    }
    public async Task<UserDto?> GetUserByEmail(
        string email,
        [Service] IUserRepository repository,
        [Service] Serilog.ILogger logger,
        [Service] Serilog.ILogger queryLogger,
        CancellationToken cancellationToken)
    {
        logger.ForContext<UserQuery>().Information("Begin: GetUserByEmail query for {Email}", email);

        try
        {
            var query = new GetUserByEmailQuery(repository, queryLogger.ForContext<GetUserByEmailQuery>());
            var user = await query.ExecuteAsync(email, cancellationToken);

            if (user != null)
            {
                logger.ForContext<UserQuery>().Information("Success: Retrieved user by email {Email}", email);
            }
            else
            {
                logger.ForContext<UserQuery>().Information("NotFound: User with email {Email} not found", email);
            }

            return user;
        }
        catch (Exception ex)
        {
            logger.ForContext<UserQuery>().Error(ex, "Error: Failed to get user by email {Email}", email);
            throw;
        }
    }
}

