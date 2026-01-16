using System.Threading;
using System.Threading.Tasks;

namespace FoodHub.Menu.Application.Interfaces;

public interface IMenuRepository
{
    Task<Domain.Entities.Menu?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Domain.Entities.Menu?> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken);
    Task AddAsync(Domain.Entities.Menu menu, CancellationToken cancellationToken);
    Task UpdateAsync(Domain.Entities.Menu menu, CancellationToken cancellationToken);
}
