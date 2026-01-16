namespace FoodHub.Menu.Infrastructure.Persistence.Cosmos;

public sealed class CosmosOptions
{
    public string Endpoint { get; init; } = default!;
    public string Key { get; init; } = default!;
    public string DatabaseName { get; init; } = default!;
    public IDictionary<string, CosmosContainerOptions>? Containers { get; init; }

    public sealed class CosmosContainerOptions
    {
        public string Name { get; init; } = default!;
    }
}
