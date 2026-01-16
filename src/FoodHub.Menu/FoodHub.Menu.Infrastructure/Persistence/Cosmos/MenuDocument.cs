using System;
using System.Collections.Generic;
using System.Linq;
using FoodHub.Menu.Domain.Entities;
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

public class MenuImageDocument
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    public static MenuImageDocument FromDomain(Domain.ValueObjects.MenuImage img)
    {
        return new MenuImageDocument
        {
            Type = img.Type,
            Url = img.Url
        };
    }

    public Domain.ValueObjects.MenuImage ToDomain()
    {
        return new Domain.ValueObjects.MenuImage(Type, Url);
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
            Availability = item.Availability.ToString()
            ,
            Images = item.Images.Select(i => MenuImageDocument.FromDomain(i)).ToList()
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
