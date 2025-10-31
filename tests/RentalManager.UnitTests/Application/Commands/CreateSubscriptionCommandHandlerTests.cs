// Copyright (c) Core. All rights reserved.

using AutoFixture;
using FluentAssertions;
using Moq;
using RentalManager.Application.Commands;
using RentalManager.Application.Handlers;
using RentalManager.Application.Interfaces;
using RentalManager.Domain.Entities;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.UnitTests.Application.Commands;

[TestFixture]
public class CreateSubscriptionCommandHandlerTests
{
    private Mock<IPaymentService> _paymentServiceMock = null!;
    private CreateSubscriptionCommandHandler _handler = null!;
    private IFixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _paymentServiceMock = new Mock<IPaymentService>();
        _handler = new CreateSubscriptionCommandHandler(_paymentServiceMock.Object);
        _fixture = new Fixture();
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldReturnSubscription()
    {
        // Arrange
        var command = new CreateSubscriptionCommand
        {
            UserId = Guid.NewGuid(),
            PlanId = "plan_123",
            Amount = 29.99m,
            Currency = "USD",
            PaymentMethodId = "pm_123",
            TrialStart = DateTime.UtcNow,
            TrialEnd = DateTime.UtcNow.AddDays(14),
        };

        var expectedPayment = new Payment(
            command.UserId,
            Money.Create(command.Amount, command.Currency),
            PaymentMethodType.Card,
            "Subscription payment");

        var expectedSubscription = new Subscription(
            command.UserId,
            command.PlanId,
            Money.Create(command.Amount, command.Currency));

        _paymentServiceMock
            .Setup(x => x.ProcessSubscriptionPaymentAsync(
                command.UserId,
                command.PlanId,
                It.Is<Money>(m => m.Amount == command.Amount && m.Currency == command.Currency),
                It.IsAny<string?>()))
            .ReturnsAsync(expectedPayment);

        _paymentServiceMock
            .Setup(x => x.GetUserSubscriptionsAsync(command.UserId, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Subscription> { expectedSubscription });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(command.UserId);
        result.PlanId.Should().Be(command.PlanId);
        result.Amount.Should().Be(command.Amount);
        result.Currency.Should().Be(command.Currency);

        _paymentServiceMock.Verify(
            x => x.ProcessSubscriptionPaymentAsync(
                command.UserId,
                command.PlanId,
                It.Is<Money>(m => m.Amount == command.Amount && m.Currency == command.Currency),
                command.PaymentMethodId),
            Times.Once);
    }

    [Test]
    public async Task Handle_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateSubscriptionCommand
        {
            UserId = Guid.Empty,
            PlanId = "plan_123",
            Amount = 29.99m,
            Currency = "USD",
        };

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("User ID cannot be empty (Parameter 'UserId')");
    }

    [Test]
    public async Task Handle_WithEmptyPlanId_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateSubscriptionCommand
        {
            UserId = Guid.NewGuid(),
            PlanId = string.Empty,
            Amount = 29.99m,
            Currency = "USD",
        };

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Plan ID cannot be null or empty (Parameter 'PlanId')");
    }

    [Test]
    public async Task Handle_WithNullPlanId_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateSubscriptionCommand
        {
            UserId = Guid.NewGuid(),
            PlanId = null!,
            Amount = 29.99m,
            Currency = "USD",
        };

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Plan ID cannot be null or empty (Parameter 'PlanId')");
    }

    [Test]
    public async Task Handle_WithZeroAmount_ShouldCallPaymentService()
    {
        // Arrange
        var command = new CreateSubscriptionCommand
        {
            UserId = Guid.NewGuid(),
            PlanId = "plan_123",
            Amount = 0m,
            Currency = "USD",
        };

        var expectedPayment = new Payment(
            command.UserId,
            Money.Create(0m, command.Currency),
            PaymentMethodType.Card,
            "Subscription payment");

        var expectedSubscription = new Subscription(
            command.UserId,
            command.PlanId,
            Money.Create(0m, command.Currency));

        _paymentServiceMock
            .Setup(x => x.ProcessSubscriptionPaymentAsync(
                command.UserId,
                command.PlanId,
                It.Is<Money>(m => m.Amount == 0m && m.Currency == command.Currency),
                It.IsAny<string?>()))
            .ReturnsAsync(expectedPayment);

        _paymentServiceMock
            .Setup(x => x.GetUserSubscriptionsAsync(command.UserId, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Subscription> { expectedSubscription });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(0m);
    }

    [Test]
    public async Task Handle_WithEmptyCurrency_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateSubscriptionCommand
        {
            UserId = Guid.NewGuid(),
            PlanId = "plan_123",
            Amount = 29.99m,
            Currency = string.Empty,
        };

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Currency cannot be null or empty (Parameter 'Currency')");
    }

    [Test]
    public async Task Handle_WithNullCurrency_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateSubscriptionCommand
        {
            UserId = Guid.NewGuid(),
            PlanId = "plan_123",
            Amount = 29.99m,
            Currency = null!,
        };

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Currency cannot be null or empty (Parameter 'Currency')");
    }

    [Test]
    public async Task Handle_WithNullPaymentMethodId_ShouldCallPaymentService()
    {
        // Arrange
        var command = new CreateSubscriptionCommand
        {
            UserId = Guid.NewGuid(),
            PlanId = "plan_123",
            Amount = 29.99m,
            Currency = "USD",
            PaymentMethodId = null,
        };

        var expectedPayment = new Payment(
            command.UserId,
            Money.Create(command.Amount, command.Currency),
            PaymentMethodType.Card,
            "Subscription payment");

        var expectedSubscription = new Subscription(
            command.UserId,
            command.PlanId,
            Money.Create(command.Amount, command.Currency));

        _paymentServiceMock
            .Setup(x => x.ProcessSubscriptionPaymentAsync(
                command.UserId,
                command.PlanId,
                It.Is<Money>(m => m.Amount == command.Amount && m.Currency == command.Currency),
                It.IsAny<string?>()))
            .ReturnsAsync(expectedPayment);

        _paymentServiceMock
            .Setup(x => x.GetUserSubscriptionsAsync(command.UserId, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Subscription> { expectedSubscription });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public async Task Handle_WhenPaymentServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateSubscriptionCommand
        {
            UserId = Guid.NewGuid(),
            PlanId = "plan_123",
            Amount = 29.99m,
            Currency = "USD",
        };

        var expectedException = new InvalidOperationException("Subscription creation failed");

        _paymentServiceMock
            .Setup(x => x.ProcessSubscriptionPaymentAsync(
                command.UserId,
                command.PlanId,
                It.IsAny<Money>(),
                It.IsAny<string?>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Subscription creation failed");
    }

    [Test]
    public async Task Handle_WhenGetUserSubscriptionsThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateSubscriptionCommand
        {
            UserId = Guid.NewGuid(),
            PlanId = "plan_123",
            Amount = 29.99m,
            Currency = "USD",
        };

        var expectedPayment = new Payment(
            command.UserId,
            Money.Create(command.Amount, command.Currency),
            PaymentMethodType.Card,
            "Subscription payment");

        _paymentServiceMock
            .Setup(x => x.ProcessSubscriptionPaymentAsync(
                command.UserId,
                command.PlanId,
                It.IsAny<Money>(),
                It.IsAny<string?>()))
            .ReturnsAsync(expectedPayment);

        var expectedException = new InvalidOperationException("Failed to retrieve subscriptions");
        _paymentServiceMock
            .Setup(x => x.GetUserSubscriptionsAsync(command.UserId, It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to retrieve subscriptions");
    }

    [Test]
    public async Task Handle_WhenSubscriptionNotFoundInUserSubscriptions_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateSubscriptionCommand
        {
            UserId = Guid.NewGuid(),
            PlanId = "plan_123",
            Amount = 29.99m,
            Currency = "USD",
        };

        var expectedPayment = new Payment(
            command.UserId,
            Money.Create(command.Amount, command.Currency),
            PaymentMethodType.Card,
            "Subscription payment");

        _paymentServiceMock
            .Setup(x => x.ProcessSubscriptionPaymentAsync(
                command.UserId,
                command.PlanId,
                It.IsAny<Money>(),
                It.IsAny<string?>()))
            .ReturnsAsync(expectedPayment);

        _paymentServiceMock
            .Setup(x => x.GetUserSubscriptionsAsync(command.UserId, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Subscription>()); // Empty list

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Created subscription not found in user subscriptions");
    }

    [Test]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToService()
    {
        // Arrange
        var command = new CreateSubscriptionCommand
        {
            UserId = Guid.NewGuid(),
            PlanId = "plan_123",
            Amount = 29.99m,
            Currency = "USD",
        };

        var expectedPayment = new Payment(
            command.UserId,
            Money.Create(command.Amount, command.Currency),
            PaymentMethodType.Card,
            "Subscription payment");

        var expectedSubscription = new Subscription(
            command.UserId,
            command.PlanId,
            Money.Create(command.Amount, command.Currency));

        _paymentServiceMock
            .Setup(x => x.ProcessSubscriptionPaymentAsync(
                command.UserId,
                command.PlanId,
                It.IsAny<Money>(),
                It.IsAny<string?>()))
            .ReturnsAsync(expectedPayment);

        _paymentServiceMock
            .Setup(x => x.GetUserSubscriptionsAsync(command.UserId, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Subscription> { expectedSubscription });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}
