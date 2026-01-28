using FluentAssertions;
using FoodHub.Menu.Domain.Entities;
using FoodHub.Menu.Domain.Enums;
using FoodHub.Menu.Domain.ValueObjects;
using Xunit;

namespace FoodHub.Menu.Domain.Tests.Entities;

public sealed class MenuItemTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateMenuItem()
    {
        // Arrange
        var name = "Margherita Pizza";
        var description = "Classic pizza with tomato and mozzarella";
        var price = new Price(12.99m, "USD");
        var category = ItemCategory.Main;

        // Act
        var menuItem = MenuItem.Create(name, description, price, category);

        // Assert
        menuItem.Should().NotBeNull();
        menuItem.Id.Should().NotBeEmpty();
        menuItem.Name.Should().Be(name);
        menuItem.Description.Should().Be(description);
        menuItem.Price.Should().Be(price);
        menuItem.Category.Should().Be(category);
        menuItem.Availability.Should().Be(ItemAvailability.Available);
        menuItem.Images.Should().BeEmpty();
    }

    [Fact]
    public void Update_WithNewValues_ShouldUpdateSuccessfully()
    {
        // Arrange
        var menuItem = CreateSampleMenuItem();
        var newPrice = new Price(15.99m, "USD");
        var newName = "Updated Item";
        var newDescription = "Updated description";

        // Act
        menuItem.Update(newName, newDescription, newPrice, ItemCategory.Dessert);

        // Assert
        menuItem.Name.Should().Be(newName);
        menuItem.Description.Should().Be(newDescription);
        menuItem.Price.Should().Be(newPrice);
        menuItem.Category.Should().Be(ItemCategory.Dessert);
    }

    [Fact]
    public void SetAvailability_WithNewAvailability_ShouldUpdateSuccessfully()
    {
        // Arrange
        var menuItem = CreateSampleMenuItem();

        // Act
        menuItem.SetAvailability(ItemAvailability.Unavailable);

        // Assert
        menuItem.Availability.Should().Be(ItemAvailability.Unavailable);
    }

    [Fact]
    public void Create_WithImages_ShouldIncludeImages()
    {
        // Arrange
        var images = new List<MenuImage>
        {
            new MenuImage("thumbnail", "https://example.com/thumb.jpg"),
            new MenuImage("full", "https://example.com/full.jpg")
        };

        // Act
        var menuItem = MenuItem.Create(
            "Pizza",
            "Delicious pizza",
            new Price(12.99m, "USD"),
            ItemCategory.Main,
            images
        );

        // Assert
        menuItem.Images.Should().HaveCount(2);
        menuItem.Images.Should().Contain(images);
    }

    private static MenuItem CreateSampleMenuItem()
    {
        return MenuItem.Create(
            "Test Item",
            "Test Description",
            new Price(10.00m, "USD"),
            ItemCategory.Main
        );
    }
}
