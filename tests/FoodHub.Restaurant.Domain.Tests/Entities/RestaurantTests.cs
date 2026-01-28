using FluentAssertions;
using FoodHub.Restaurant.Domain.ValueObjects;
using Xunit;

namespace FoodHub.Restaurant.Domain.Tests.Entities;

public sealed class RestaurantTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateRestaurant()
    {
        // Arrange
        var name = new RestaurantName("The Great Restaurant");
        var city = "New York";

        // Act
        var restaurant = new Domain.Entities.Restaurant(name, city);

        // Assert
        restaurant.Should().NotBeNull();
        restaurant.Id.Should().NotBeEmpty();
        restaurant.Name.Should().Be(name);
        restaurant.City.Should().Be(city);
        restaurant.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithFullParameters_ShouldCreateRestaurant()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = new RestaurantName("Test Restaurant");
        var city = "Boston";
        var isActive = false;

        // Act
        var restaurant = new Domain.Entities.Restaurant(id, name, city, isActive);

        // Assert
        restaurant.Id.Should().Be(id);
        restaurant.Name.Should().Be(name);
        restaurant.City.Should().Be(city);
        restaurant.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrowDomainException()
    {
        // Arrange
        var name = new RestaurantName("Test");
        var city = "Test City";

        // Act
        Action act = () => new Domain.Entities.Restaurant(Guid.Empty, name, city, true);

        // Assert
        act.Should().Throw<Exceptions.DomainException>()
            .WithMessage("Id must not be empty.");
    }

    [Fact]
    public void Constructor_WithEmptyCity_ShouldThrowDomainException()
    {
        // Arrange
        var name = new RestaurantName("Test Restaurant");

        // Act
        Action act = () => new Domain.Entities.Restaurant(name, "");

        // Assert
        act.Should().Throw<Exceptions.DomainException>()
            .WithMessage("City is required.");
    }

    [Fact]
    public void ChangeName_WithNewName_ShouldUpdateSuccessfully()
    {
        // Arrange
        var restaurant = CreateSampleRestaurant();
        var newName = new RestaurantName("Updated Restaurant Name");

        // Act
        restaurant.ChangeName(newName);

        // Assert
        restaurant.Name.Should().Be(newName);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var restaurant = CreateSampleRestaurant();
        restaurant.IsActive.Should().BeTrue(); // Precondition

        // Act
        restaurant.Deactivate();

        // Assert
        restaurant.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var restaurant = CreateSampleRestaurant();
        restaurant.Deactivate();
        restaurant.IsActive.Should().BeFalse(); // Precondition

        // Act
        restaurant.Activate();

        // Assert
        restaurant.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldRemainInactive()
    {
        // Arrange
        var restaurant = CreateSampleRestaurant();
        restaurant.Deactivate();

        // Act
        restaurant.Deactivate();

        // Assert
        restaurant.IsActive.Should().BeFalse();
    }

    private static Domain.Entities.Restaurant CreateSampleRestaurant()
    {
        return new Domain.Entities.Restaurant(
            new RestaurantName("Sample Restaurant"),
            "Sample City"
        );
    }
}
