using FluentAssertions;
using FoodHub.User.Domain.ValueObjects;
using FoodHub.User.Domain.Exceptions;
using Xunit;

namespace FoodHub.User.Domain.Tests.Entities;

public sealed class UserTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        var email = new Email("test@example.com");
        var name = "Test User";
        var phone = "1234567890";

        // Act
        var user = new Domain.Entities.User(name, email, phone);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be(email);
        user.Name.Should().Be(name);
        user.Phone.Should().Be(phone);
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void UpdateProfile_WithNewNameAndPhone_ShouldUpdateSuccessfully()
    {
        // Arrange
        var user = CreateSampleUser();
        var newName = "Updated Name";
        var newPhone = "9876543210";

        // Act
        user.UpdateProfile(newName, newPhone);

        // Assert
        user.Name.Should().Be(newName);
        user.Phone.Should().Be(newPhone);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateProfile_WithInvalidName_ShouldThrowDomainException(string invalidName)
    {
        // Arrange
        var user = CreateSampleUser();

        // Act
        Action act = () => user.UpdateProfile(invalidName, "1234567890");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("User name cannot be empty.");
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = CreateSampleUser();
        user.IsActive.Should().BeTrue(); // Precondition

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = CreateSampleUser();
        user.Deactivate();
        user.IsActive.Should().BeFalse(); // Precondition

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateEmail_WithNewEmail_ShouldUpdate()
    {
        // Arrange
        var user = CreateSampleUser();
        var newEmail = new Email("newemail@example.com");

        // Act
        user.UpdateEmail(newEmail);

        // Assert
        user.Email.Should().Be(newEmail);
    }

    private static Domain.Entities.User CreateSampleUser()
    {
        return new Domain.Entities.User(
            "Sample User",
            new Email("sample@example.com"),
            "1234567890"
        );
    }
}
