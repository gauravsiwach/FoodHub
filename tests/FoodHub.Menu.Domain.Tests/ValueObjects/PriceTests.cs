using FluentAssertions;
using FoodHub.Menu.Domain.Exceptions;
using FoodHub.Menu.Domain.ValueObjects;
using Xunit;

namespace FoodHub.Menu.Domain.Tests.ValueObjects;

public sealed class PriceTests
{
    [Theory]
    [InlineData(0.01, "USD")]
    [InlineData(10.50, "EUR")]
    [InlineData(99.99, "GBP")]
    [InlineData(1000.00, "INR")]
    public void Create_WithValidValues_ShouldReturnPrice(decimal amount, string currency)
    {
        // Act
        var price = new Price(amount, currency);

        // Assert
        price.Should().NotBeNull();
        price.Amount.Should().Be(amount);
        price.Currency.Should().Be(currency);
    }

    [Theory]
    [InlineData(-1.00)]
    [InlineData(-0.01)]
    public void Create_WithNegativeAmount_ShouldThrowDomainException(decimal negativeAmount)
    {
        // Act
        Action act = () => new Price(negativeAmount, "USD");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Price amount cannot be negative.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCurrency_ShouldThrowDomainException(string invalidCurrency)
    {
        // Act
        Action act = () => new Price(10.00m, invalidCurrency);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Currency cannot be empty.");
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldSucceed()
    {
        // Act
        var price = new Price(0m, "USD");

        // Assert
        price.Amount.Should().Be(0m);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var price1 = new Price(10.50m, "USD");
        var price2 = new Price(10.50m, "USD");

        // Act & Assert
        price1.Should().Be(price2);
    }

    [Fact]
    public void Equals_WithDifferentCurrency_ShouldReturnFalse()
    {
        // Arrange
        var price1 = new Price(10.50m, "USD");
        var price2 = new Price(10.50m, "EUR");

        // Act & Assert
        price1.Should().NotBe(price2);
    }
}
