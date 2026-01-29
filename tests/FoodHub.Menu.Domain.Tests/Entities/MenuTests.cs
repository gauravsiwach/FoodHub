using FluentAssertions;
using FoodHub.Menu.Domain.Entities;
using FoodHub.Menu.Domain.Enums;
using FoodHub.Menu.Domain.ValueObjects;
using Xunit;

namespace FoodHub.Menu.Domain.Tests.Entities;

public sealed class MenuTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateMenu()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var name = "Lunch Menu";
        var description = "Delicious lunch options";

        // Act
        var menu = Domain.Entities.Menu.Create(restaurantId, name, description);

        // Assert
        menu.Should().NotBeNull();
        menu.Id.Should().NotBeEmpty();
        menu.RestaurantId.Should().Be(restaurantId);
        menu.Name.Should().Be(name);
        menu.Description.Should().Be(description);
        menu.MenuItems.Should().BeEmpty();
    }

    [Fact]
    public void AddMenuItem_WithValidMenuItem_ShouldAddToCollection()
    {
        // Arrange
        var menu = CreateSampleMenu();
        var menuItem = CreateSampleMenuItem("Burger");

        // Act
        menu.AddMenuItem(menuItem);

        // Assert
        menu.MenuItems.Should().HaveCount(1);
        menu.MenuItems.Should().Contain(menuItem);
    }

    [Fact]
    public void UpdateMenuItem_WithValidParameters_ShouldUpdateSuccessfully()
    {
        // Arrange
        var menu = CreateSampleMenu();
        var menuItem = CreateSampleMenuItem("Pizza");
        menu.AddMenuItem(menuItem);
        var newPrice = new Price(15.99m, "USD");

        // Act
        menu.UpdateMenuItem(menuItem.Id, "Updated Pizza", "New description", newPrice, ItemCategory.Dessert);

        // Assert
        var updatedItem = menu.MenuItems.First(mi => mi.Id == menuItem.Id);
        updatedItem.Name.Should().Be("Updated Pizza");
        updatedItem.Description.Should().Be("New description");
        updatedItem.Price.Should().Be(newPrice);
    }

    private static Domain.Entities.Menu CreateSampleMenu()
    {
        return Domain.Entities.Menu.Create(
            Guid.NewGuid(),
            "Test Menu",
            "Test Description"
        );
    }

    private static MenuItem CreateSampleMenuItem(string name)
    {
        return MenuItem.Create(
            name,
            "Test item description",
            new Price(10.00m, "USD"),
            ItemCategory.Main
        );
    }
}
