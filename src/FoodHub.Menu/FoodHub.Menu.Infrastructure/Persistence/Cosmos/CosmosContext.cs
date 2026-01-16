using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace FoodHub.Menu.Infrastructure.Persistence.Cosmos;

public sealed class CosmosContext
{
    private readonly CosmosClient _client;
    private readonly CosmosOptions _options;

    public Container Container { get; }

    public CosmosContext(
        CosmosClient client,
        IOptions<CosmosOptions> options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        var containerName = ResolveContainerName("Menu");
        Container = _client.GetContainer(_options.DatabaseName, containerName);
    }

    public Container GetContainer(string containerName)
    {
        if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentException("containerName is required", nameof(containerName));
        return _client.GetContainer(_options.DatabaseName, containerName);
    }

    private string ResolveContainerName(string key)
    {
        if (_options.Containers != null && _options.Containers.TryGetValue(key, out var c) && !string.IsNullOrWhiteSpace(c?.Name))
        {
            return c!.Name;
        }

        throw new InvalidOperationException($"Cosmos container name for '{key}' is not configured under Cosmos:Containers.");
    }
}
