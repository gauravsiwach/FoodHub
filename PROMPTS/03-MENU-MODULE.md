# Prompt 3: Menu Module Implementation

## Context
Implement the **Menu Module** following Clean Architecture and DDD principles. This module manages menu and menu item aggregates with Cosmos DB persistence and cross-module validation.

## Module Characteristics
- **Business Type:** Read-heavy, volatile domain with complex business rules
- **Database:** Azure Cosmos DB
- **Container:** `Menus`
- **Partition Strategy:** `/restaurantId` (restaurant-scoped queries)
- **Cross-Module:** Consumes `IRestaurantReadRepository` from Restaurant module

---

## Part 1: Cross-Module Interface

### 1.1 Read Repository Interface (`FoodHub.Menu.Application/Interfaces/IRestaurantReadRepository.cs`)
```csharp
namespace FoodHub.Menu.Application.Interfaces;

// This interface is defined in Menu module but implemented in Restaurant module
// Provides clean, decoupled cross-module validation
public interface IRestaurantReadRepository
{
    Task<bool> ExistsAsync(Guid restaurantId, CancellationToken cancellationToken);
}
```

---

## Part 2: Domain Layer (FoodHub.Menu.Domain)

### 2.1 Domain Exception (`Exceptions/DomainException.cs`)
```csharp
namespace FoodHub.Menu.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

### 2.2 Enums (`Enums/ItemCategory.cs`, `Enums/ItemAvailability.cs`)
```csharp
namespace FoodHub.Menu.Domain.Enums;

public enum ItemCategory
{
    Appetizer,
    Main,
    Dessert,
    Beverage,
    Side
}

public enum ItemAvailability
{
    Available,
    Unavailable
}
```

### 2.3 Value Objects

**Price.cs:**
```csharp
using FoodHub.Menu.Domain.Exceptions;

namespace FoodHub.Menu.Domain.ValueObjects;

public record Price
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Price(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("Price amount cannot be negative.");
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency cannot be empty.");

        Amount = amount;
        Currency = currency;
    }
}
```

**MenuImage.cs:**
```csharp
using FoodHub.Menu.Domain.Exceptions;

namespace FoodHub.Menu.Domain.ValueObjects;

public record MenuImage
{
    public string Type { get; }
    public string Url { get; }

    public MenuImage(string type, string url)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new DomainException("Image type is required.");
        
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Image URL is required.");

        Type = type;
        Url = url;
    }
}
```

### 2.4 MenuItem Entity (`Entities/MenuItem.cs`)
```csharp
using FoodHub.Menu.Domain.Enums;
using FoodHub.Menu.Domain.ValueObjects;
using FoodHub.Menu.Domain.Exceptions;

namespace FoodHub.Menu.Domain.Entities;

public class MenuItem
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Price Price { get; private set; }
    public ItemCategory Category { get; private set; }
    public ItemAvailability Availability { get; private set; }
    
    private readonly List<MenuImage> _images = new();
    public IReadOnlyList<MenuImage> Images => _images.AsReadOnly();

    private MenuItem() { }

    public static MenuItem Create(
        string name, 
        string description, 
        Price price, 
        ItemCategory category, 
        IEnumerable<MenuImage>? images = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Menu item name cannot be empty.");

        var item = new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            Category = category,
            Availability = ItemAvailability.Available
        };

        if (images is not null)
            item._images.AddRange(images);

        return item;
    }

    public static MenuItem Rehydrate(
        Guid id, 
        string name, 
        string description, 
        Price price, 
        ItemCategory category, 
        ItemAvailability availability, 
        IEnumerable<MenuImage>? images = null)
    {
        if (id == Guid.Empty) 
            throw new DomainException("MenuItem id is required for rehydration.");
        
        if (string.IsNullOrWhiteSpace(name)) 
            throw new DomainException("Menu item name cannot be empty.");

        var item = new MenuItem
        {
            Id = id,
            Name = name,
            Description = description,
            Price = price,
            Category = category,
            Availability = availability
        };

        if (images is not null)
            item._images.AddRange(images);

        return item;
    }

    public void Update(
        string name, 
        string description, 
        Price price, 
        ItemCategory category, 
        IEnumerable<MenuImage>? images = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Menu item name cannot be empty.");
        
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        
        if (images is not null)
        {
            _images.Clear();
            _images.AddRange(images);
        }
    }

    public void SetAvailability(ItemAvailability availability)
    {
        Availability = availability;
    }
}
```

### 2.5 Menu Aggregate Root (`Entities/Menu.cs`)
```csharp
using FoodHub.Menu.Domain.Exceptions;

namespace FoodHub.Menu.Domain.Entities;

public class Menu
{
    public Guid Id { get; private set; }
    public Guid RestaurantId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }

    private readonly List<MenuItem> _menuItems = new();
    public IReadOnlyList<MenuItem> MenuItems => _menuItems.AsReadOnly();

    private Menu() { }

    public static Menu Create(Guid restaurantId, string name, string? description = null)
    {
        if (restaurantId == Guid.Empty)
            throw new DomainException("A menu must be associated with a valid restaurant.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Menu name cannot be empty.");

        return new Menu
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            Name = name,
            Description = description
        };
    }

    public static Menu Rehydrate(
        Guid id, 
        Guid restaurantId, 
        string name, 
        string? description, 
        IEnumerable<MenuItem>? items = null)
    {
        if (id == Guid.Empty) 
            throw new DomainException("Menu id is required for rehydration.");
        
        if (restaurantId == Guid.Empty) 
            throw new DomainException("RestaurantId is required for rehydration.");
        
        if (string.IsNullOrWhiteSpace(name)) 
            throw new DomainException("Menu name cannot be empty.");

        var menu = new Menu
        {
            Id = id,
            RestaurantId = restaurantId,
            Name = name,
            Description = description
        };

        if (items is not null)
        {
            foreach (var it in items)
                menu._menuItems.Add(it);
        }

        return menu;
    }

    public void AddMenuItem(MenuItem item)
    {
        if (_menuItems.Any(mi => mi.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Menu item with name '{item.Name}' already exists.");

        _menuItems.Add(item);
    }

    public void UpdateMenuItem(
        Guid itemId, 
        string name, 
        string description, 
        ValueObjects.Price price, 
        Enums.ItemCategory category, 
        IEnumerable<ValueObjects.MenuImage>? images = null)
    {
        var existingItem = _menuItems.FirstOrDefault(mi => mi.Id == itemId);
        if (existingItem is null)
            throw new DomainException($"Menu item with ID '{itemId}' not found.");

        // Check for name collision if the name is being changed
        if (!existingItem.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
            _menuItems.Any(mi => mi.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException($"Menu item with name '{name}' already exists.");
        }

        existingItem.Update(name, description, price, category, images);
    }
}
```

---

## Part 3: Application Layer (FoodHub.Menu.Application)

### 3.1 DTOs (`Dtos/`)
```csharp
namespace FoodHub.Menu.Application.Dtos;

public record MenuDto(
    Guid Id,
    Guid RestaurantId,
    string Name,
    string? Description,
    IReadOnlyList<MenuItemDto> MenuItems,
    MenuPriceDto? PriceRange);

public record MenuItemDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    string Category,
    string Availability,
    IReadOnlyList<MenuImageDto> Images);

public record MenuImageDto(string Type, string Url);

public record MenuPriceDto(decimal MinPrice, decimal MaxPrice, string Currency);
```

### 3.2 Repository Interface (`Interfaces/IMenuRepository.cs`)
```csharp
namespace FoodHub.Menu.Application.Interfaces;

public interface IMenuRepository
{
    Task<Domain.Entities.Menu?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Domain.Entities.Menu?> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken);
    Task AddAsync(Domain.Entities.Menu menu, CancellationToken cancellationToken);
    Task UpdateAsync(Domain.Entities.Menu menu, CancellationToken cancellationToken);
}
```

### 3.3 Create Menu Command (`Commands/CreateMenuCommand.cs`)
```csharp
using FoodHub.Menu.Application.Interfaces;
using FoodHub.Menu.Application.Dtos;
using Serilog;

namespace FoodHub.Menu.Application.Commands;

public record CreateMenuItemDto(
    string Name, 
    string Description, 
    decimal Price, 
    string Currency, 
    Domain.Enums.ItemCategory Category, 
    IReadOnlyList<MenuImageDto>? Images = null);

public record CreateMenuDto(
    Guid RestaurantId, 
    string Name, 
    string? Description = null, 
    IReadOnlyList<CreateMenuItemDto>? Items = null);

public class CreateMenuCommand
{
    private readonly IMenuRepository _menuRepository;
    private readonly IRestaurantReadRepository _restaurantReadRepository;
    private readonly Serilog.ILogger _logger;

    public CreateMenuCommand(
        IMenuRepository menuRepository,
        IRestaurantReadRepository restaurantReadRepository,
        Serilog.ILogger logger)
    {
        _menuRepository = menuRepository ?? throw new ArgumentNullException(nameof(menuRepository));
        _restaurantReadRepository = restaurantReadRepository ?? throw new ArgumentNullException(nameof(restaurantReadRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> ExecuteAsync(CreateMenuDto dto, CancellationToken cancellationToken)
    {
        _logger.Information("Use Case: Creating menu for Restaurant {RestaurantId}", dto.RestaurantId);

        // Cross-module validation
        var restaurantExists = await _restaurantReadRepository.ExistsAsync(dto.RestaurantId, cancellationToken);
        if (!restaurantExists)
        {
            throw new Exceptions.ApplicationException($"Restaurant with ID '{dto.RestaurantId}' does not exist.");
        }

        var menu = Domain.Entities.Menu.Create(dto.RestaurantId, dto.Name, dto.Description);

        if (dto.Items is not null && dto.Items.Count > 0)
        {
            foreach (var it in dto.Items)
            {
                var price = new Domain.ValueObjects.Price(it.Price, it.Currency);
                IEnumerable<Domain.ValueObjects.MenuImage>? domainImages = null;
                if (it.Images is not null)
                    domainImages = it.Images.Select(i => new Domain.ValueObjects.MenuImage(i.Type, i.Url));

                var menuItem = Domain.Entities.MenuItem.Create(it.Name, it.Description, price, it.Category, domainImages);
                menu.AddMenuItem(menuItem);
            }
        }

        await _menuRepository.AddAsync(menu, cancellationToken);

        _logger.Information("Successfully created Menu {MenuId} for Restaurant {RestaurantId}", 
            menu.Id, menu.RestaurantId);

        return menu.Id;
    }
}
```

### 3.4 Get Menu Query (`Queries/GetMenuByIdQuery.cs`)
```csharp
using FoodHub.Menu.Application.Dtos;
using FoodHub.Menu.Application.Interfaces;
using Serilog;

namespace FoodHub.Menu.Application.Queries;

public class GetMenuByIdQuery
{
    private readonly IMenuRepository _repository;
    private readonly Serilog.ILogger _logger;

    public GetMenuByIdQuery(IMenuRepository repository, Serilog.ILogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MenuDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.Information("Use Case: Retrieving menu with Id {MenuId}", id);

        var menu = await _repository.GetByIdAsync(id, cancellationToken);
        if (menu is null)
        {
            _logger.Warning("Menu with Id {MenuId} not found", id);
            return null;
        }

        MenuPriceDto? priceRange = null;
        if (menu.MenuItems.Any())
        {
            var minPrice = menu.MenuItems.Min(mi => mi.Price.Amount);
            var maxPrice = menu.MenuItems.Max(mi => mi.Price.Amount);
            var currency = menu.MenuItems.First().Price.Currency;
            priceRange = new MenuPriceDto(minPrice, maxPrice, currency);
        }

        var menuItemDtos = menu.MenuItems
            .Select(mi => new MenuItemDto(
                mi.Id, 
                mi.Name, 
                mi.Description, 
                mi.Price.Amount, 
                mi.Price.Currency, 
                mi.Category.ToString(), 
                mi.Availability.ToString(), 
                mi.Images.Select(i => new MenuImageDto(i.Type, i.Url)).ToList()))
            .ToList();

        return new MenuDto(
            menu.Id, 
            menu.RestaurantId, 
            menu.Name, 
            menu.Description, 
            menuItemDtos, 
            priceRange);
    }
}
```

---

## Part 4: Infrastructure Layer (FoodHub.Menu.Infrastructure)

### 4.1 Cosmos Options & Context (Same as Restaurant Module)
Use same structure as Restaurant module but with Menu container.

### 4.2 Menu Document (`Persistence/Cosmos/MenuDocument.cs`)
```csharp
using Newtonsoft.Json;

namespace FoodHub.Menu.Infrastructure.Persistence.Cosmos;

public class MenuDocument
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("restaurantId")]
    public Guid RestaurantId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("items")]
    public List<MenuItemDocument> Items { get; set; } = new();

    public static MenuDocument FromDomain(Domain.Entities.Menu menu)
    {
        return new MenuDocument
        {
            Id = menu.Id,
            RestaurantId = menu.RestaurantId,
            Name = menu.Name,
            Description = menu.Description,
            Items = menu.MenuItems.Select(MenuItemDocument.FromDomain).ToList()
        };
    }

    public Domain.Entities.Menu ToDomain()
    {
        var items = Items?.Select(i => i.ToDomain()).ToList();
        return Domain.Entities.Menu.Rehydrate(Id, RestaurantId, Name, Description, items);
    }
}

public class MenuItemDocument
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("priceAmount")]
    public decimal PriceAmount { get; set; }

    [JsonProperty("priceCurrency")]
    public string PriceCurrency { get; set; }

    [JsonProperty("category")]
    public string Category { get; set; }

    [JsonProperty("availability")]
    public string Availability { get; set; }

    [JsonProperty("images")]
    public List<MenuImageDocument> Images { get; set; } = new();

    public static MenuItemDocument FromDomain(Domain.Entities.MenuItem item)
    {
        return new MenuItemDocument
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            PriceAmount = item.Price.Amount,
            PriceCurrency = item.Price.Currency,
            Category = item.Category.ToString(),
            Availability = item.Availability.ToString(),
            Images = item.Images.Select(MenuImageDocument.FromDomain).ToList()
        };
    }

    public Domain.Entities.MenuItem ToDomain()
    {
        var price = new Domain.ValueObjects.Price(PriceAmount, PriceCurrency);
        var category = (Domain.Enums.ItemCategory)Enum.Parse(typeof(Domain.Enums.ItemCategory), Category);
        var availability = (Domain.Enums.ItemAvailability)Enum.Parse(typeof(Domain.Enums.ItemAvailability), Availability);
        var images = Images?.Select(i => i.ToDomain()).ToList();
        return Domain.Entities.MenuItem.Rehydrate(Id, Name, Description, price, category, availability, images);
    }
}

public class MenuImageDocument
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    public static MenuImageDocument FromDomain(Domain.ValueObjects.MenuImage image)
    {
        return new MenuImageDocument { Type = image.Type, Url = image.Url };
    }

    public Domain.ValueObjects.MenuImage ToDomain()
    {
        return new Domain.ValueObjects.MenuImage(Type, Url);
    }
}
```

### 4.3 Menu Repository (`Persistence/Repositories/MenuRepository.cs`)
```csharp
using Microsoft.Azure.Cosmos;
using FoodHub.Menu.Application.Interfaces;
using FoodHub.Menu.Infrastructure.Persistence.Cosmos;
using MenuEntity = FoodHub.Menu.Domain.Entities.Menu;

namespace FoodHub.Menu.Infrastructure.Persistence.Repositories;

public class MenuRepository : IMenuRepository
{
    private readonly Container _container;
    private readonly Serilog.ILogger _logger;

    public MenuRepository(CosmosContext context, Serilog.ILogger logger)
    {
        _logger = logger?.ForContext<MenuRepository>() ?? throw new ArgumentNullException(nameof(logger));
        if (context is null) throw new ArgumentNullException(nameof(context));

        _container = context.Container;
    }

    public async Task<MenuEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.Debug("Fetching Menu by id {MenuId} from Cosmos", id);

        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", id);
        
        var iterator = _container.GetItemQueryIterator<MenuDocument>(query);
        
        while (iterator.HasMoreResults)
        {
            var resp = await iterator.ReadNextAsync(cancellationToken);
            var doc = resp.Resource.FirstOrDefault();
            if (doc != null)
                return doc.ToDomain();
        }

        return null;
    }

    public async Task<MenuEntity?> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken)
    {
        _logger.Debug("Fetching Menu for Restaurant {RestaurantId} from Cosmos", restaurantId);

        var query = new QueryDefinition("SELECT * FROM c WHERE c.restaurantId = @rid")
            .WithParameter("@rid", restaurantId);
        
        var iterator = _container.GetItemQueryIterator<MenuDocument>(
            query, 
            requestOptions: new QueryRequestOptions 
            { 
                PartitionKey = new PartitionKey(restaurantId.ToString()) 
            });
        
        while (iterator.HasMoreResults)
        {
            var resp = await iterator.ReadNextAsync(cancellationToken);
            var doc = resp.Resource.FirstOrDefault();
            if (doc != null)
                return doc.ToDomain();
        }

        return null;
    }

    public async Task AddAsync(MenuEntity menu, CancellationToken cancellationToken)
    {
        _logger.Information("Inserting Menu {MenuId} into Cosmos", menu.Id);
        var doc = MenuDocument.FromDomain(menu);
        await _container.CreateItemAsync(
            doc, 
            new PartitionKey(menu.RestaurantId.ToString()), 
            cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(MenuEntity menu, CancellationToken cancellationToken)
    {
        _logger.Information("Upserting Menu {MenuId} into Cosmos", menu.Id);
        var doc = MenuDocument.FromDomain(menu);
        await _container.UpsertItemAsync(
            doc, 
            new PartitionKey(menu.RestaurantId.ToString()), 
            cancellationToken: cancellationToken);
    }
}
```

### 4.4 Restaurant Read Repository (Cross-Module Implementation)
In **FoodHub.Restaurant.Infrastructure**, create:

**`Persistence/Repositories/RestaurantReadRepository.cs`:**
```csharp
using FoodHub.Menu.Application.Interfaces;
using FoodHub.Restaurant.Infrastructure.Persistence.Cosmos;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace FoodHub.Restaurant.Infrastructure.Persistence.Repositories;

public class RestaurantReadRepository : IRestaurantReadRepository
{
    private readonly Container _container;

    public RestaurantReadRepository(CosmosContext context)
    {
        _container = context.Container;
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

1. Complete Menu module with Domain, Application, Infrastructure layers
2. Cross-module interface defined in Menu.Application
3. Cross-module implementation in Restaurant.Infrastructure
4. MenuItem and Menu entities with business logic
5. Value objects (Price, MenuImage) with validation
6. Cosmos DB document mapping with nested structures
7. Repository with restaurant-scoped partitioning

## Success Criteria

- Menu module builds successfully
- Cross-module dependency properly implemented
- Restaurant validation works during menu creation
- Cosmos DB partitioning by restaurantId
- All business rules enforced in domain layer
- Menu items properly managed within aggregate
