using FluentAssertions;
using FoodHub.User.Domain.Exceptions;
using FoodHub.User.Domain.ValueObjects;
using Xunit;

namespace FoodHub.User.Domain.Tests.ValueObjects;

public sealed class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@domain.co.uk")]
    [InlineData("user+tag@gmail.com")]
    [InlineData("123@test.com")]
    public void Create_WithValidEmail_ShouldReturnEmail(string validEmail)
    {
        // Act
        var email = new Email(validEmail);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be(validEmail.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldThrowDomainException(string invalidEmail)
    {
        // Act
        Action act = () => new Email(invalidEmail);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Email cannot be empty.");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    [InlineData("user @domain.com")]
    [InlineData("user@.com")]
    public void Create_WithInvalidFormat_ShouldThrowDomainException(string invalidFormat)
    {
        // Act
        Action act = () => new Email(invalidFormat);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Email format is invalid.");
    }

    [Fact]
    public void Create_ShouldNormalizeToLowerCase()
    {
        // Arrange
        var upperCaseEmail = "USER@EXAMPLE.COM";

        // Act
        var email = new Email(upperCaseEmail);

        // Assert
        email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");

        // Act & Assert
        email1.Should().Be(email2);
    }

    [Fact]
    public void Equals_WithDifferentCase_ShouldReturnTrue()
    {
        // Arrange
        var email1 = new Email("Test@Example.COM");
        var email2 = new Email("test@example.com");

        // Act & Assert
        email1.Should().Be(email2); // Both normalized to lowercase
    }
}
