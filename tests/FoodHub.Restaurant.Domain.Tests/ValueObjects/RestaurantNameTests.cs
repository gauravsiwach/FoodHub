using FluentAssertions;
using FoodHub.Restaurant.Domain.Exceptions;
using FoodHub.Restaurant.Domain.ValueObjects;
using Xunit;

namespace FoodHub.Restaurant.Domain.Tests.ValueObjects;

public sealed class RestaurantNameTests
{
    [Theory]
    [InlineData("McDonald's")]
    [InlineData("Burger King")]
    [InlineData("A&W")]
    [InlineData("123 Restaurant")]
    public void Constructor_WithValidName_ShouldReturnRestaurantName(string validName)
    {
        // Act
        var result = new RestaurantName(validName);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(validName.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespace_ShouldThrowDomainException(string invalidName)
    {
        // Act
        Action act = () => new RestaurantName(invalidName);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Restaurant name must not be empty.");
    }

    [Fact]
    public void Constructor_ShouldTrimWhitespace()
    {
        // Arrange
        var nameWithSpaces = "  Pizza Hut  ";

        // Act
        var result = new RestaurantName(nameWithSpaces);

        // Assert
        result.Value.Should().Be("Pizza Hut");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var name1 = new RestaurantName("Pizza Hut");
        var name2 = new RestaurantName("Pizza Hut");

        // Act & Assert
        name1.Should().Be(name2);
        (name1 == name2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var name1 = new RestaurantName("Pizza Hut");
        var name2 = new RestaurantName("Domino's");

        // Act & Assert
        name1.Should().NotBe(name2);
        (name1 != name2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_ToStringshould_ReturnValue()
    {
        // Arrange
        var name = new RestaurantName("Test Restaurant");

        // Act
        string result = name;

        // Assert
        result.Should().Be("Test Restaurant");
    }
}
