using System;
using System.Threading;
using System.Threading.Tasks;
using UserEntity = FoodHub.User.Domain.Entities.User;

namespace FoodHub.User.Application.Interfaces;

public interface IUserRepository
{
    Task AddAsync(UserEntity user, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserEntity user, CancellationToken cancellationToken = default);
}