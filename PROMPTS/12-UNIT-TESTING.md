# Prompt 12: Unit Testing (Domain & Application Layers)

## Overview
Implement comprehensive unit tests for Domain and Application layers across all three modules using xUnit, FluentAssertions, and NSubstitute.

## Why Unit Tests for This Architecture?

- **Modular Monolith**: Each module's business logic must be testable in isolation
- **Domain-Driven Design**: Validate value objects, entity invariants, and business rules
- **Cross-Module Dependencies**: Mock interfaces to test behavior without external dependencies
- **Regression Prevention**: Catch breaking changes before deployment
- **Documentation**: Tests serve as executable specifications

---

## Testing Tools & Packages

### NuGet Packages (Add to Each Test Project)
```xml
<ItemGroup>
  <PackageReference Include="xUnit" Version="2.9.3" />
  <PackageReference Include="xUnit.runner.visualstudio" Version="2.8.2" />
  <PackageReference Include="FluentAssertions" Version="6.12.2" />
  <PackageReference Include="NSubstitute" Version="5.3.0" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
</ItemGroup>
```

### Tool Purposes
- **xUnit**: Modern .NET testing framework with clean syntax
- **FluentAssertions**: Readable assertion syntax (`result.Should().BeEquivalentTo(expected)`)
- **NSubstitute**: Simple mocking library for interfaces and abstract classes

---

## Test Project Structure

Add to your solution:
```
tests/
├── FoodHub.Restaurant.Domain.Tests/
│   ├── Entities/
│   │   └── RestaurantTests.cs
│   ├── ValueObjects/
│   │   └── RestaurantNameTests.cs
│   └── FoodHub.Restaurant.Domain.Tests.csproj
├── FoodHub.Restaurant.Application.Tests/
│   ├── Commands/
│   │   └── CreateRestaurantCommandTests.cs
│   ├── Queries/
│   │   └── GetRestaurantByIdQueryTests.cs
│   └── FoodHub.Restaurant.Application.Tests.csproj
├── FoodHub.Menu.Domain.Tests/
│   ├── Entities/
│   │   ├── MenuTests.cs
│   │   └── MenuItemTests.cs
│   ├── ValueObjects/
│   │   ├── PriceTests.cs
│   │   └── MenuImageTests.cs
│   └── FoodHub.Menu.Domain.Tests.csproj
├── FoodHub.Menu.Application.Tests/
│   ├── Commands/
│   │   ├── CreateMenuCommandTests.cs
│   │   └── AddMenuItemCommandTests.cs
│   └── FoodHub.Menu.Application.Tests.csproj
├── FoodHub.User.Domain.Tests/
│   ├── Entities/
│   │   └── UserTests.cs
│   ├── ValueObjects/
│   │   └── EmailTests.cs
│   └── FoodHub.User.Domain.Tests.csproj
└── FoodHub.User.Application.Tests/
    ├── Commands/
    │   └── CreateUserCommandTests.cs
    └── FoodHub.User.Application.Tests.csproj
```

---

## Domain Layer Tests

### 1. Restaurant Module - Value Object Tests

**File**: `tests/FoodHub.Restaurant.Domain.Tests/ValueObjects/RestaurantNameTests.cs`

```csharp
using FluentAssertions;
using FoodHub.Restaurant.Domain.Exceptions;
using FoodHub.Restaurant.Domain.ValueObjects;

namespace FoodHub.Restaurant.Domain.Tests.ValueObjects;

public sealed class RestaurantNameTests
{
    [Theory]
    [InlineData("McDonald's")]
    [InlineData("Burger King")]
    [InlineData("A&W")]
    [InlineData("123 Restaurant")]
    public void Create_WithValidName_ShouldReturnRestaurantName(string validName)
    {
        // Act
        var result = RestaurantName.Create(validName);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(validName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrWhitespace_ShouldThrowDomainException(string invalidName)
    {
        // Act
        Action act = () => RestaurantName.Create(invalidName);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Restaurant name cannot be empty");
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("A")]
    public void Create_WithNameTooShort_ShouldThrowDomainException(string shortName)
    {
        // Act
        Action act = () => RestaurantName.Create(shortName);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Restaurant name must be at least 3 characters");
    }

    [Fact]
    public void Create_WithNameTooLong_ShouldThrowDomainException()
    {
        // Arrange
        var longName = new string('A', 101); // 101 characters

        // Act
        Action act = () => RestaurantName.Create(longName);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Restaurant name cannot exceed 100 characters");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var name1 = RestaurantName.Create("Pizza Hut");
        var name2 = RestaurantName.Create("Pizza Hut");

        // Act & Assert
        name1.Should().Be(name2);
        (name1 == name2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var name1 = RestaurantName.Create("Pizza Hut");
        var name2 = RestaurantName.Create("Domino's");

        // Act & Assert
        name1.Should().NotBe(name2);
        (name1 != name2).Should().BeTrue();
    }
}
```

### 2. Restaurant Module - Entity Tests

**File**: `tests/FoodHub.Restaurant.Domain.Tests/Entities/RestaurantTests.cs`

```csharp
using FluentAssertions;
using FoodHub.Restaurant.Domain.Entities;
using FoodHub.Restaurant.Domain.ValueObjects;

namespace FoodHub.Restaurant.Domain.Tests.Entities;

public sealed class RestaurantTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateRestaurant()
    {
        // Arrange
        var name = RestaurantName.Create("The Great Restaurant");
        var address = "123 Main St, City, Country";
        var phoneNumber = "+1234567890";

        // Act
        var restaurant = Entities.Restaurant.Create(name, address, phoneNumber);

        // Assert
        restaurant.Should().NotBeNull();
        restaurant.Id.Should().NotBeEmpty();
        restaurant.Name.Should().Be(name);
        restaurant.Address.Should().Be(address);
        restaurant.PhoneNumber.Should().Be(phoneNumber);
        restaurant.IsActive.Should().BeTrue();
        restaurant.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void UpdateAddress_WithNewAddress_ShouldUpdateSuccessfully()
    {
        // Arrange
        var restaurant = CreateSampleRestaurant();
        var newAddress = "456 New Street, New City";

        // Act
        restaurant.UpdateAddress(newAddress);

        // Assert
        restaurant.Address.Should().Be(newAddress);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateAddress_WithInvalidAddress_ShouldThrowArgumentException(string invalidAddress)
    {
        // Arrange
        var restaurant = CreateSampleRestaurant();

        // Act
        Action act = () => restaurant.UpdateAddress(invalidAddress);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Address cannot be empty*");
    }

    [Fact]
    public void UpdatePhoneNumber_WithNewPhone_ShouldUpdateSuccessfully()
    {
        // Arrange
        var restaurant = CreateSampleRestaurant();
        var newPhone = "+9876543210";

        // Act
        restaurant.UpdatePhoneNumber(newPhone);

        // Assert
        restaurant.PhoneNumber.Should().Be(newPhone);
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

    private static Entities.Restaurant CreateSampleRestaurant()
    {
        return Entities.Restaurant.Create(
            RestaurantName.Create("Sample Restaurant"),
            "123 Sample St",
            "+1234567890"
        );
    }
}
```

### 3. Menu Module - Value Object Tests

**File**: `tests/FoodHub.Menu.Domain.Tests/ValueObjects/PriceTests.cs`

```csharp
using FluentAssertions;
using FoodHub.Menu.Domain.Exceptions;
using FoodHub.Menu.Domain.ValueObjects;

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
        var price = Price.Create(amount, currency);

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
        Action act = () => Price.Create(negativeAmount, "USD");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Price amount must be greater than or equal to zero");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidCurrency_ShouldThrowDomainException(string invalidCurrency)
    {
        // Act
        Action act = () => Price.Create(10.00m, invalidCurrency);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Currency cannot be empty");
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldSucceed()
    {
        // Act
        var price = Price.Create(0m, "USD");

        // Assert
        price.Amount.Should().Be(0m);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var price1 = Price.Create(10.50m, "USD");
        var price2 = Price.Create(10.50m, "USD");

        // Act & Assert
        price1.Should().Be(price2);
    }

    [Fact]
    public void Equals_WithDifferentCurrency_ShouldReturnFalse()
    {
        // Arrange
        var price1 = Price.Create(10.50m, "USD");
        var price2 = Price.Create(10.50m, "EUR");

        // Act & Assert
        price1.Should().NotBe(price2);
    }
}
```

### 4. Menu Module - Entity Tests

**File**: `tests/FoodHub.Menu.Domain.Tests/Entities/MenuItemTests.cs`

```csharp
using FluentAssertions;
using FoodHub.Menu.Domain.Entities;
using FoodHub.Menu.Domain.Enums;
using FoodHub.Menu.Domain.ValueObjects;

namespace FoodHub.Menu.Domain.Tests.Entities;

public sealed class MenuItemTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateMenuItem()
    {
        // Arrange
        var name = "Margherita Pizza";
        var description = "Classic pizza with tomato and mozzarella";
        var price = Price.Create(12.99m, "USD");
        var category = ItemCategory.MainCourse;

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
        menuItem.Image.Should().BeNull();
    }

    [Fact]
    public void UpdatePrice_WithNewPrice_ShouldUpdateSuccessfully()
    {
        // Arrange
        var menuItem = CreateSampleMenuItem();
        var newPrice = Price.Create(15.99m, "USD");

        // Act
        menuItem.UpdatePrice(newPrice);

        // Assert
        menuItem.Price.Should().Be(newPrice);
    }

    [Fact]
    public void UpdatePrice_WithNullPrice_ShouldThrowArgumentNullException()
    {
        // Arrange
        var menuItem = CreateSampleMenuItem();

        // Act
        Action act = () => menuItem.UpdatePrice(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MarkAsUnavailable_WhenAvailable_ShouldChangeToUnavailable()
    {
        // Arrange
        var menuItem = CreateSampleMenuItem();
        menuItem.Availability.Should().Be(ItemAvailability.Available); // Precondition

        // Act
        menuItem.MarkAsUnavailable();

        // Assert
        menuItem.Availability.Should().Be(ItemAvailability.Unavailable);
    }

    [Fact]
    public void MarkAsAvailable_WhenUnavailable_ShouldChangeToAvailable()
    {
        // Arrange
        var menuItem = CreateSampleMenuItem();
        menuItem.MarkAsUnavailable();
        menuItem.Availability.Should().Be(ItemAvailability.Unavailable); // Precondition

        // Act
        menuItem.MarkAsAvailable();

        // Assert
        menuItem.Availability.Should().Be(ItemAvailability.Available);
    }

    [Fact]
    public void SetImage_WithValidImage_ShouldUpdateImage()
    {
        // Arrange
        var menuItem = CreateSampleMenuItem();
        var image = MenuImage.Create("https://example.com/pizza.jpg", "Delicious pizza");

        // Act
        menuItem.SetImage(image);

        // Assert
        menuItem.Image.Should().Be(image);
    }

    [Fact]
    public void UpdateDescription_WithNewDescription_ShouldUpdate()
    {
        // Arrange
        var menuItem = CreateSampleMenuItem();
        var newDescription = "Updated description with more details";

        // Act
        menuItem.UpdateDescription(newDescription);

        // Assert
        menuItem.Description.Should().Be(newDescription);
    }

    private static MenuItem CreateSampleMenuItem()
    {
        return MenuItem.Create(
            "Test Item",
            "Test Description",
            Price.Create(10.00m, "USD"),
            ItemCategory.MainCourse
        );
    }
}
```

### 5. User Module - Value Object Tests

**File**: `tests/FoodHub.User.Domain.Tests/ValueObjects/EmailTests.cs`

```csharp
using FluentAssertions;
using FoodHub.User.Domain.Exceptions;
using FoodHub.User.Domain.ValueObjects;

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
        var email = Email.Create(validEmail);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrWhitespace_ShouldThrowDomainException(string invalidEmail)
    {
        // Act
        Action act = () => Email.Create(invalidEmail);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Email cannot be empty");
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
        Action act = () => Email.Create(invalidFormat);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid email format");
    }

    [Fact]
    public void Create_ShouldNormalizeToLowerCase()
    {
        // Arrange
        var upperCaseEmail = "USER@EXAMPLE.COM";

        // Act
        var email = Email.Create(upperCaseEmail);

        // Assert
        email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        email1.Should().Be(email2);
    }

    [Fact]
    public void Equals_WithDifferentCase_ShouldReturnTrue()
    {
        // Arrange
        var email1 = Email.Create("Test@Example.COM");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        email1.Should().Be(email2); // Both normalized to lowercase
    }
}
```

---

## Application Layer Tests (Command Handlers)

### 6. Restaurant Module - Command Handler Tests

**File**: `tests/FoodHub.Restaurant.Application.Tests/Commands/CreateRestaurantCommandTests.cs`

```csharp
using FluentAssertions;
using NSubstitute;
using FoodHub.Restaurant.Application.Commands;
using FoodHub.Restaurant.Application.Interfaces;
using FoodHub.Restaurant.Domain.ValueObjects;

namespace FoodHub.Restaurant.Application.Tests.Commands;

public sealed class CreateRestaurantCommandTests
{
    private readonly IRestaurantRepository _mockRepository;

    public CreateRestaurantCommandTests()
    {
        _mockRepository = Substitute.For<IRestaurantRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateRestaurant()
    {
        // Arrange
        var command = new CreateRestaurantCommand
        {
            Name = "New Restaurant",
            Address = "123 Test Street",
            PhoneNumber = "+1234567890"
        };

        Domain.Entities.Restaurant? savedRestaurant = null;
        _mockRepository.SaveAsync(Arg.Do<Domain.Entities.Restaurant>(r => savedRestaurant = r), 
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await CreateRestaurantCommandHandler.HandleAsync(
            command, 
            _mockRepository, 
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.Address.Should().Be(command.Address);
        result.PhoneNumber.Should().Be(command.PhoneNumber);

        await _mockRepository.Received(1).SaveAsync(
            Arg.Is<Domain.Entities.Restaurant>(r => 
                r.Name.Value == command.Name && 
                r.Address == command.Address),
            Arg.Any<CancellationToken>());

        savedRestaurant.Should().NotBeNull();
        savedRestaurant!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithInvalidName_ShouldThrowDomainException()
    {
        // Arrange
        var command = new CreateRestaurantCommand
        {
            Name = "AB", // Too short
            Address = "123 Test Street",
            PhoneNumber = "+1234567890"
        };

        // Act
        Func<Task> act = async () => await CreateRestaurantCommandHandler.HandleAsync(
            command, 
            _mockRepository, 
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .Where(e => e.Message.Contains("Restaurant name"));

        await _mockRepository.DidNotReceive().SaveAsync(
            Arg.Any<Domain.Entities.Restaurant>(), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallRepositorySaveOnce()
    {
        // Arrange
        var command = new CreateRestaurantCommand
        {
            Name = "Test Restaurant",
            Address = "Test Address",
            PhoneNumber = "+1111111111"
        };

        // Act
        await CreateRestaurantCommandHandler.HandleAsync(
            command, 
            _mockRepository, 
            CancellationToken.None);

        // Assert
        await _mockRepository.Received(1).SaveAsync(
            Arg.Any<Domain.Entities.Restaurant>(), 
            Arg.Any<CancellationToken>());
    }
}
```

### 7. Menu Module - Command Handler with Cross-Module Validation

**File**: `tests/FoodHub.Menu.Application.Tests/Commands/CreateMenuCommandTests.cs`

```csharp
using FluentAssertions;
using NSubstitute;
using FoodHub.Menu.Application.Commands;
using FoodHub.Menu.Application.Exceptions;
using FoodHub.Menu.Application.Interfaces;

namespace FoodHub.Menu.Application.Tests.Commands;

public sealed class CreateMenuCommandTests
{
    private readonly IMenuRepository _mockMenuRepository;
    private readonly IRestaurantReadRepository _mockRestaurantRepository;

    public CreateMenuCommandTests()
    {
        _mockMenuRepository = Substitute.For<IMenuRepository>();
        _mockRestaurantRepository = Substitute.For<IRestaurantReadRepository>();
    }

    [Fact]
    public async Task Handle_WithValidRestaurantId_ShouldCreateMenu()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var command = new CreateMenuCommand
        {
            RestaurantId = restaurantId,
            Name = "Lunch Menu",
            Description = "Delicious lunch options"
        };

        // Mock: Restaurant exists and is active
        _mockRestaurantRepository.ExistsAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRestaurantRepository.IsActiveAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(true);

        Domain.Entities.Menu? savedMenu = null;
        _mockMenuRepository.SaveAsync(
            Arg.Do<Domain.Entities.Menu>(m => savedMenu = m), 
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await CreateMenuCommandHandler.HandleAsync(
            command,
            _mockMenuRepository,
            _mockRestaurantRepository,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.RestaurantId.Should().Be(restaurantId);

        savedMenu.Should().NotBeNull();
        savedMenu!.Items.Should().BeEmpty();

        await _mockRestaurantRepository.Received(1).ExistsAsync(restaurantId, Arg.Any<CancellationToken>());
        await _mockMenuRepository.Received(1).SaveAsync(Arg.Any<Domain.Entities.Menu>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentRestaurant_ShouldThrowApplicationException()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var command = new CreateMenuCommand
        {
            RestaurantId = restaurantId,
            Name = "Lunch Menu",
            Description = "Test"
        };

        // Mock: Restaurant does not exist
        _mockRestaurantRepository.ExistsAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Func<Task> act = async () => await CreateMenuCommandHandler.HandleAsync(
            command,
            _mockMenuRepository,
            _mockRestaurantRepository,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage($"Restaurant with ID {restaurantId} does not exist");

        await _mockMenuRepository.DidNotReceive().SaveAsync(
            Arg.Any<Domain.Entities.Menu>(), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInactiveRestaurant_ShouldThrowApplicationException()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var command = new CreateMenuCommand
        {
            RestaurantId = restaurantId,
            Name = "Lunch Menu",
            Description = "Test"
        };

        // Mock: Restaurant exists but is inactive
        _mockRestaurantRepository.ExistsAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(true);
        _mockRestaurantRepository.IsActiveAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Func<Task> act = async () => await CreateMenuCommandHandler.HandleAsync(
            command,
            _mockMenuRepository,
            _mockRestaurantRepository,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage($"Restaurant with ID {restaurantId} is not active");

        await _mockMenuRepository.DidNotReceive().SaveAsync(
            Arg.Any<Domain.Entities.Menu>(), 
            Arg.Any<CancellationToken>());
    }
}
```

### 8. User Module - Query Handler Tests

**File**: `tests/FoodHub.User.Application.Tests/Queries/GetUserByEmailQueryTests.cs`

```csharp
using FluentAssertions;
using NSubstitute;
using FoodHub.User.Application.Interfaces;
using FoodHub.User.Application.Queries;
using FoodHub.User.Domain.ValueObjects;

namespace FoodHub.User.Application.Tests.Queries;

public sealed class GetUserByEmailQueryTests
{
    private readonly IUserRepository _mockRepository;

    public GetUserByEmailQueryTests()
    {
        _mockRepository = Substitute.For<IUserRepository>();
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnUser()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var query = new GetUserByEmailQuery { Email = email.Value };

        var existingUser = Domain.Entities.User.Create(
            email,
            "Test User",
            "google-sub-123"
        );

        _mockRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        var result = await GetUserByEmailQueryHandler.HandleAsync(
            query,
            _mockRepository,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.Name.Should().Be("Test User");

        await _mockRepository.Received(1).GetByEmailAsync(email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ShouldReturnNull()
    {
        // Arrange
        var email = Email.Create("nonexistent@example.com");
        var query = new GetUserByEmailQuery { Email = email.Value };

        _mockRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns((Domain.Entities.User?)null);

        // Act
        var result = await GetUserByEmailQueryHandler.HandleAsync(
            query,
            _mockRepository,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();

        await _mockRepository.Received(1).GetByEmailAsync(email, Arg.Any<CancellationToken>());
    }
}
```

---

## Test Project Configuration

### Example: FoodHub.Restaurant.Domain.Tests.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xUnit" Version="2.9.3" />
    <PackageReference Include="xUnit.runner.visualstudio" Version="2.8.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" Version="6.12.2" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\FoodHub.Restaurant\FoodHub.Restaurant.Domain\FoodHub.Restaurant.Domain.csproj" />
  </ItemGroup>

</Project>
```

---

## Running Tests

### Visual Studio
- **Test Explorer**: View → Test Explorer → Run All

### Command Line
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/FoodHub.Restaurant.Domain.Tests/

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### VS Code
- Install **C# Dev Kit** extension
- Use Testing sidebar (flask icon)
- Run/Debug individual tests

---

## Best Practices

### 1. Test Naming Convention
```
MethodName_Scenario_ExpectedBehavior
```
Examples:
- `Create_WithValidName_ShouldReturnRestaurantName`
- `UpdatePrice_WithNullPrice_ShouldThrowArgumentNullException`

### 2. AAA Pattern (Arrange-Act-Assert)
```csharp
[Fact]
public void TestMethod()
{
    // Arrange: Set up test data and mocks
    var input = "test";
    
    // Act: Execute the method under test
    var result = MethodUnderTest(input);
    
    // Assert: Verify expected outcome
    result.Should().Be("expected");
}
```

### 3. Test Independence
- Each test should be self-contained
- No shared state between tests
- Tests can run in any order

### 4. Mock Only Interfaces
- Mock dependencies (repositories, external services)
- Don't mock domain entities or value objects
- Test domain logic directly

### 5. FluentAssertions Syntax
```csharp
// Instead of Assert.Equal
result.Should().Be(expected);

// Instead of Assert.NotNull
result.Should().NotBeNull();

// Collection assertions
list.Should().HaveCount(3);
list.Should().Contain(item => item.Id == expectedId);

// Exception assertions
act.Should().Throw<DomainException>()
    .WithMessage("Expected message");
```

---

## Success Criteria

After implementing unit tests, verify:

✅ **Domain Layer Tests**
- All value objects have validation tests
- Entity business logic is tested
- Edge cases covered (null, empty, boundary values)

✅ **Application Layer Tests**
- Command handlers tested with mocked repositories
- Query handlers return expected DTOs
- Cross-module validation tested (e.g., Menu → Restaurant)

✅ **Test Execution**
- All tests pass with `dotnet test`
- No flaky tests (tests pass consistently)
- Test coverage > 80% for Domain and Application layers

✅ **Code Quality**
- Test names clearly describe scenarios
- AAA pattern followed consistently
- Mocks used only for dependencies, not domain objects

✅ **CI/CD Ready**
- Tests run in GitHub Actions pipeline (optional)
- Fast execution (< 5 seconds for unit tests)

---

## Next Steps

After unit tests are complete:
- **Integration Tests** (Prompt 13): Test with real Cosmos DB and SQL Server using TestContainers
- **API Tests**: Test GraphQL endpoints with WebApplicationFactory
- **Performance Tests**: Load testing with K6 or Apache JMeter

---

## Key Takeaways

- **Unit tests validate business rules** in isolation
- **Mock cross-module dependencies** to test behavior independently
- **FluentAssertions** makes tests more readable
- **Fast feedback loop**: Unit tests run in milliseconds
- **Regression prevention**: Catch breaking changes early

This comprehensive test suite ensures your modular monolith is robust and maintainable!
