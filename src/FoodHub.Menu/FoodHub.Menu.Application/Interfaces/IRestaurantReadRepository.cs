using System.Threading;
using System.Threading.Tasks;

namespace FoodHub.Menu.Application.Interfaces;

// This interface is defined here but implemented in the FoodHub.Restaurant module.
// It provides a clean, decoupled way to perform read-only checks across module boundaries.
public interface IRestaurantReadRepository
{
    Task<bool> ExistsAsync(Guid restaurantId, CancellationToken cancellationToken);
}
