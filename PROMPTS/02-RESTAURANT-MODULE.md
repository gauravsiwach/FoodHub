# Prompt 2: Restaurant Module Implementation

## Context
Implement the **Restaurant Module** following Clean Architecture and Domain-Driven Design principles. This module manages restaurant aggregates with Cosmos DB persistence.

## Module Characteristics
- **Business Type:** Write-light, stable domain
- **Database:** Azure Cosmos DB
- **Container:** `Restaurants`
- **Partition Strategy:** `/id` (restaurant ID)
- **Cross-Module:** Exposes read-only interface for Menu module

---

## Part 1: Domain Layer (FoodHub.Restaurant.Domain)

### Package Dependencies
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

### 1.1 Domain Exception (`Exceptions/DomainException.cs`)
```csharp
namespace FoodHub.Restaurant.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

### 1.2 Value Object: RestaurantName (`ValueObjects/RestaurantName.cs`)
```csharp
namespace FoodHub.Restaurant.Domain.ValueObjects;

public sealed record RestaurantName
{
    public string Value { get; }

    public RestaurantName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new Exceptions.DomainException("Restaurant name must not be empty.");

        Value = value.Trim();
    }

    public override string ToString() => Value;

    public static implicit operator string(RestaurantName name) => name.Value;
    public static explicit operator RestaurantName(string value) => new RestaurantName(value);
}
```

### 1.3 Aggregate Root: Restaurant (`Entities/Restaurant.cs`)
```csharp
using FoodHub.Restaurant.Domain.ValueObjects;

namespace FoodHub.Restaurant.Domain.Entities;

public sealed class Restaurant
{
    public Guid Id { get; private set; }
    public RestaurantName Name { get; private set; }
    public string City { get; private set; }
    public bool IsActive { get; private set; }

    // Private constructor for persistence
    private Restaurant() { }

    // Factory method for creating new restaurants
    public Restaurant(RestaurantName name, string city)
    {
        if (name is null)
            throw new Exceptions.DomainException("Restaurant name is required.");

        if (string.IsNullOrWhiteSpace(city))
            throw new Exceptions.DomainException("City is required.");

        Id = Guid.NewGuid();
        Name = name;
        City = city.Trim();
        IsActive = true;
    }

    // Business operations
    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }

    public void UpdateName(RestaurantName newName)
    {
        if (newName is null)
            throw new Exceptions.DomainException("Restaurant name cannot be null.");
        
        Name = newName;
    }

    public void UpdateCity(string newCity)
    {
        if (string.IsNullOrWhiteSpace(newCity))
            throw new Exceptions.DomainException("City cannot be empty.");
        
        City = newCity.Trim();
    }
}
```

---

## Part 2: Application Layer (FoodHub.Restaurant.Application)

### Package Dependencies
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\FoodHub.Restaurant.Domain\FoodHub.Restaurant.Domain.csproj" />
  </ItemGroup>
</Project>
```

### 2.1 Repository Interface (`Interfaces/IRestaurantRepository.cs`)
```csharp
using RestaurantEntity = FoodHub.Restaurant.Domain.Entities.Restaurant;

namespace FoodHub.Restaurant.Application.Interfaces;

public interface IRestaurantRepository
{
    Task AddAsync(RestaurantEntity restaurant, CancellationToken cancellationToken = default);
    Task<RestaurantEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<RestaurantEntity>> GetAllAsync(CancellationToken cancellationToken = default);
}
```

### 2.2 DTO (`Dtos/RestaurantDto.cs`)
```csharp
namespace FoodHub.Restaurant.Application.Dtos;

public record RestaurantDto(Guid Id, string Name, string City, bool IsActive);

public record CreateRestaurantDto(string Name, string City);
```

### 2.3 Create Command (`Commands/CreateRestaurant/CreateRestaurantCommand.cs`)
```csharp
using FoodHub.Restaurant.Application.Dtos;
using FoodHub.Restaurant.Application.Interfaces;
using FoodHub.Restaurant.Domain.ValueObjects;

namespace FoodHub.Restaurant.Application.Commands.CreateRestaurant;

public sealed class CreateRestaurantCommand
{
    private readonly IRestaurantRepository _repository;

    public CreateRestaurantCommand(IRestaurantRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Guid> ExecuteAsync(CreateRestaurantDto dto, CancellationToken cancellationToken = default)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        var name = new RestaurantName(dto.Name);
        var restaurant = new Domain.Entities.Restaurant(name, dto.City);

        await _repository.AddAsync(restaurant, cancellationToken).ConfigureAwait(false);

        return restaurant.Id;
    }
}
```

### 2.4 Get By ID Query (`Queries/GetRestaurantById/GetRestaurantByIdQuery.cs`)
```csharp
using FoodHub.Restaurant.Application.Dtos;
using FoodHub.Restaurant.Application.Interfaces;

namespace FoodHub.Restaurant.Application.Queries.GetRestaurantById;

public sealed class GetRestaurantByIdQuery
{
    private readonly IRestaurantRepository _repository;

    public GetRestaurantByIdQuery(IRestaurantRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<RestaurantDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var restaurant = await _repository.GetByIdAsync(id, cancellationToken);
        
        if (restaurant is null)
            return null;

        return new RestaurantDto(restaurant.Id, restaurant.Name.Value, restaurant.City, restaurant.IsActive);
    }
}
```

### 2.5 Get All Query (`Queries/GetAllRestaurants/GetAllRestaurantsQuery.cs`)
```csharp
using FoodHub.Restaurant.Application.Dtos;
using FoodHub.Restaurant.Application.Interfaces;

namespace FoodHub.Restaurant.Application.Queries.GetAllRestaurants;

public sealed class GetAllRestaurantsQuery
{
    private readonly IRestaurantRepository _repository;

    public GetAllRestaurantsQuery(IRestaurantRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IEnumerable<RestaurantDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var restaurants = await _repository.GetAllAsync(cancellationToken);
        
        return restaurants.Select(r => new RestaurantDto(r.Id, r.Name.Value, r.City, r.IsActive));
    }
}
```

---

## Part 3: Infrastructure Layer (FoodHub.Restaurant.Infrastructure)

### Package Dependencies
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.Azure.Cosmos" Version="3.56.0" />
    <PackageReference Include="Microsoft.Extensions.Options" Version="10.0.1" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\FoodHub.Restaurant.Application\FoodHub.Restaurant.Application.csproj" />
    <ProjectReference Include="..\FoodHub.Restaurant.Domain\FoodHub.Restaurant.Domain.csproj" />
  </ItemGroup>
</Project>
```

### 3.1 Cosmos Options (`Persistence/Cosmos/CosmosOptions.cs`)
```csharp
namespace FoodHub.Restaurant.Infrastructure.Persistence.Cosmos;

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
```

### 3.2 Cosmos Context (`Persistence/Cosmos/CosmosContext.cs`)
```csharp
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace FoodHub.Restaurant.Infrastructure.Persistence.Cosmos;

public sealed class CosmosContext
{
    private readonly CosmosClient _client;
    private readonly CosmosOptions _options;

    public Container Container { get; }

    public CosmosContext(CosmosClient client, IOptions<CosmosOptions> options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        var containerName = ResolveContainerName("Restaurant");
        Container = _client.GetContainer(_options.DatabaseName, containerName);
    }

    public Container GetContainer(string containerName)
    {
        if (string.IsNullOrWhiteSpace(containerName)) 
            throw new ArgumentException("containerName is required", nameof(containerName));
        
        return _client.GetContainer(_options.DatabaseName, containerName);
    }

    private string ResolveContainerName(string key)
    {
        if (_options.Containers != null && 
            _options.Containers.TryGetValue(key, out var c) && 
            !string.IsNullOrWhiteSpace(c?.Name))
        {
            return c!.Name;
        }

        throw new InvalidOperationException(
            $"Cosmos container name for '{key}' is not configured under Cosmos:Containers.");
    }
}
```

### 3.3 Restaurant Document (`Persistence/Cosmos/RestaurantDocument.cs`)
```csharp
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
```

### 3.4 Restaurant Repository (`Persistence/Repositories/RestaurantRepository.cs`)
```csharp
using FoodHub.Restaurant.Application.Interfaces;
using FoodHub.Restaurant.Domain.ValueObjects;
using FoodHub.Restaurant.Infrastructure.Persistence.Cosmos;
using Microsoft.Azure.Cosmos;
using RestaurantEntity = FoodHub.Restaurant.Domain.Entities.Restaurant;
using System.Net;

namespace FoodHub.Restaurant.Infrastructure.Persistence.Repositories;

public sealed class RestaurantRepository : IRestaurantRepository
{
    private readonly Container _container;

    public RestaurantRepository(CosmosContext context)
    {
        _container = context.Container;
    }

    public async Task AddAsync(RestaurantEntity restaurant, CancellationToken cancellationToken = default)
    {
        var document = new RestaurantDocument
        {
            Id = restaurant.Id.ToString(),
            Name = restaurant.Name.Value,
            City = restaurant.City,
            IsActive = restaurant.IsActive
        };

        await _container.CreateItemAsync(
            document,
            new PartitionKey(document.Id),
            cancellationToken: cancellationToken);
    }

    public async Task<RestaurantEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<RestaurantDocument>(
                id.ToString(),
                new PartitionKey(id.ToString()),
                cancellationToken: cancellationToken);

            var doc = response.Resource;

            return new RestaurantEntity(
                Guid.Parse(doc.Id),
                new RestaurantName(doc.Name),
                doc.City,
                doc.IsActive);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<RestaurantEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = _container.GetItemQueryIterator<RestaurantDocument>(query);
        var results = new List<RestaurantEntity>();

        while (iterator.HasMoreResults)
        {
            var resp = await iterator.ReadNextAsync(cancellationToken);
            foreach (var doc in resp.Resource)
            {
                results.Add(new RestaurantEntity(
                    Guid.Parse(doc.Id),
                    new RestaurantName(doc.Name),
                    doc.City,
                    doc.IsActive));
            }
        }

        return results;
    }

    public async Task<bool> ExistsAsync(Guid restaurantId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<RestaurantDocument>(
                restaurantId.ToString(),
                new PartitionKey(restaurantId.ToString()),
                cancellationToken: cancellationToken);

            return response.StatusCode == HttpStatusCode.OK;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}
```

---

## Expected Output

1. **Domain Layer:**
   - DomainException class
   - RestaurantName value object with validation
   - Restaurant aggregate root with business operations
   
2. **Application Layer:**
   - IRestaurantRepository interface
   - RestaurantDto and CreateRestaurantDto
   - CreateRestaurantCommand
   - GetRestaurantByIdQuery
   - GetAllRestaurantsQuery

3. **Infrastructure Layer:**
   - CosmosOptions configuration class
   - CosmosContext for container resolution
   - RestaurantDocument persistence model
   - RestaurantRepository implementation with CRUD operations

## Success Criteria

- All projects build successfully
- Domain layer has no external dependencies
- Application layer only references Domain
- Infrastructure implements Application interfaces
- Cosmos DB integration with proper document mapping
- Value object validation works correctly
- Repository pattern properly implemented
