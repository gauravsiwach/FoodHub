using FluentAssertions;
using NSubstitute;
using FoodHub.Menu.Application.Commands;
using FoodHub.Menu.Application.Interfaces;
using Serilog;
using Xunit;

namespace FoodHub.Menu.Application.Tests.Commands;

public sealed class CreateMenuCommandTests
{
    private readonly IMenuRepository _mockMenuRepository;
    private readonly IRestaurantReadRepository _mockRestaurantRepository;
    private readonly ILogger _mockLogger;

    public CreateMenuCommandTests()
    {
        _mockMenuRepository = Substitute.For<IMenuRepository>();
        _mockRestaurantRepository = Substitute.For<IRestaurantReadRepository>();
        _mockLogger = Substitute.For<ILogger>();
        _mockLogger.ForContext<CreateMenuCommand>().Returns(_mockLogger);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRestaurantId_ShouldCreateMenu()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var dto = new CreateMenuDto(restaurantId, "Lunch Menu", "Delicious lunch options");

        _mockRestaurantRepository.ExistsAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateMenuCommand(_mockMenuRepository, _mockRestaurantRepository, _mockLogger);

        // Act
        var menuId = await command.ExecuteAsync(dto, CancellationToken.None);

        // Assert
        menuId.Should().NotBeEmpty();
        await _mockRestaurantRepository.Received(1).ExistsAsync(restaurantId, Arg.Any<CancellationToken>());
        await _mockMenuRepository.Received(1).AddAsync(Arg.Any<Domain.Entities.Menu>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentRestaurant_ShouldThrowApplicationException()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var dto = new CreateMenuDto(restaurantId, "Lunch Menu", "Test");

        _mockRestaurantRepository.ExistsAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateMenuCommand(_mockMenuRepository, _mockRestaurantRepository, _mockLogger);

        // Act
        Func<Task> act = async () => await command.ExecuteAsync(dto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Application.Exceptions.ApplicationException>()
            .WithMessage($"Restaurant with ID '{restaurantId}' does not exist.");

        await _mockMenuRepository.DidNotReceive().AddAsync(
            Arg.Any<Domain.Entities.Menu>(), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMenuItems_ShouldCreateMenuWithItems()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var items = new List<CreateMenuItemDto>
        {
            new CreateMenuItemDto("Pizza", "Delicious pizza", 12.99m, "USD", Domain.Enums.ItemCategory.Main)
        };
        var dto = new CreateMenuDto(restaurantId, "Lunch Menu", "Test", items);

        _mockRestaurantRepository.ExistsAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateMenuCommand(_mockMenuRepository, _mockRestaurantRepository, _mockLogger);

        // Act
        var menuId = await command.ExecuteAsync(dto, CancellationToken.None);

        // Assert
        menuId.Should().NotBeEmpty();
        await _mockMenuRepository.Received(1).AddAsync(
            Arg.Is<Domain.Entities.Menu>(m => m.MenuItems.Count == 1), 
            Arg.Any<CancellationToken>());
    }
}
