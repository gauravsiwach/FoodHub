using Newtonsoft.Json;

namespace FoodHub.Restaurant.Infrastructure.Persistence.Cosmos;

public sealed class RestaurantDocument
{
    [JsonProperty("id")]
    public string Id { get; init; } = default!;

    [JsonProperty("name")]
    public string Name { get; init; } = default!;

    [JsonProperty("city")]
    public string City { get; init; } = default!;

    [JsonProperty("isActive")]
    public bool IsActive { get; init; }
}
