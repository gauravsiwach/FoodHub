using System;
using System.Threading;
using System.Threading.Tasks;
using FoodHub.User.Application.Interfaces;
using FoodHub.User.Infrastructure.Sql.Models;
using Microsoft.EntityFrameworkCore;
using UserEntity = FoodHub.User.Domain.Entities.User;

namespace FoodHub.User.Infrastructure.Sql.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        var userEntity = Models.UserEntity.FromDomain(user);
        
        _context.Users.Add(userEntity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userEntity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return userEntity?.ToDomain();
    }

    public async Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        
        var userEntity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        return userEntity?.ToDomain();
    }

    public async Task UpdateAsync(UserEntity user, CancellationToken cancellationToken = default)
    {
        var existingEntity = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

        if (existingEntity is null)
        {
            throw new InvalidOperationException($"User with ID {user.Id} not found for update.");
        }

        // Update properties
        existingEntity.Name = user.Name;
        existingEntity.Phone = user.Phone;
        existingEntity.IsActive = user.IsActive;
        // Note: Email and CreatedAt are not updated in this use case

        await _context.SaveChangesAsync(cancellationToken);
    }
}