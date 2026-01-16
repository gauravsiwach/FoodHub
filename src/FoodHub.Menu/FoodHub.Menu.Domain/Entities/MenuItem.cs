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
    private readonly List<ValueObjects.MenuImage> _images = new();
    public IReadOnlyList<ValueObjects.MenuImage> Images => _images.AsReadOnly();

    // Private constructor for persistence mapping
    private MenuItem() { }

    public static MenuItem Create(string name, string description, Price price, ItemCategory category, IEnumerable<ValueObjects.MenuImage>? images = null)
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
        {
            item._images.AddRange(images);
        }

        return item;
    }

    // Rehydrate from persistence
    public static MenuItem Rehydrate(Guid id, string name, string description, Price price, ItemCategory category, ItemAvailability availability, IEnumerable<ValueObjects.MenuImage>? images = null)
    {
        if (id == Guid.Empty) throw new DomainException("MenuItem id is required for rehydration.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Menu item name cannot be empty.");

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
        {
            item._images.AddRange(images);
        }

        return item;
    }

    public void Update(string name, string description, Price price, ItemCategory category, IEnumerable<ValueObjects.MenuImage>? images = null)
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
