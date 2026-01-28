using FluentAssertions;
using FoodHub.Menu.Domain.Exceptions;
using FoodHub.Menu.Domain.ValueObjects;
using Xunit;

namespace FoodHub.Menu.Domain.Tests.ValueObjects;

public sealed class MenuImageTests
{
    [Theory]
    [InlineData("thumbnail", "https://example.com/image.jpg")]
    [InlineData("full", "https://cdn.food.com/burger.png")]
    public void Create_WithValidParameters_ShouldReturnMenuImage(string type, string url)
    {
        // Act
        var image = new MenuImage(type, url);

        // Assert
        image.Should().NotBeNull();
        image.Type.Should().Be(type);
        image.Url.Should().Be(url);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidType_ShouldThrowDomainException(string invalidType)
    {
        // Act
        Action act = () => new MenuImage(invalidType, "https://example.com/image.jpg");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Image type cannot be empty.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidUrl_ShouldThrowDomainException(string invalidUrl)
    {
        // Act
        Action act = () => new MenuImage("thumbnail", invalidUrl);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Image url cannot be empty.");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var image1 = new MenuImage("thumbnail", "https://example.com/image.jpg");
        var image2 = new MenuImage("thumbnail", "https://example.com/image.jpg");

        // Act & Assert
        image1.Should().Be(image2);
    }
}
