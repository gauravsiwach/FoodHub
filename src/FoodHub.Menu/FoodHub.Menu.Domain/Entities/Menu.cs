using FoodHub.Menu.Domain.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace FoodHub.Menu.Domain.Entities;

public class Menu
{
    public Guid Id { get; private set; }
    public Guid RestaurantId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }

    private readonly List<MenuItem> _menuItems = new();
    public IReadOnlyList<MenuItem> MenuItems => _menuItems.AsReadOnly();

    // Private constructor for persistence mapping
    private Menu() { }

    public static Menu Create(Guid restaurantId, string name, string? description = null)
    {
        if (restaurantId == Guid.Empty)
            throw new DomainException("A menu must be associated with a valid restaurant.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Menu name cannot be empty.");

        var menu = new Menu
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            Name = name,
            Description = description,
        };

        return menu;
    }

    // Rehydrate from persistence - keeps domain logic inside the domain layer
    public static Menu Rehydrate(Guid id, Guid restaurantId, string name, string? description, IEnumerable<MenuItem>? items = null)
    {
        if (id == Guid.Empty) throw new DomainException("Menu id is required for rehydration.");
        if (restaurantId == Guid.Empty) throw new DomainException("RestaurantId is required for rehydration.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Menu name cannot be empty.");

        var menu = new Menu
        {
            Id = id,
            RestaurantId = restaurantId,
            Name = name,
            Description = description,
        };



        if (items is not null)
        {
            foreach (var it in items)
            {
                menu._menuItems.Add(it);
            }
        }

        return menu;
    }

    public void AddMenuItem(MenuItem item)
    {
        if (_menuItems.Any(mi => mi.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Menu item with name '{item.Name}' already exists.");

        _menuItems.Add(item);
    }
    
    public void UpdateMenuItem(Guid itemId, string name, string description, ValueObjects.Price price, Enums.ItemCategory category, IEnumerable<ValueObjects.MenuImage>? images = null)
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
