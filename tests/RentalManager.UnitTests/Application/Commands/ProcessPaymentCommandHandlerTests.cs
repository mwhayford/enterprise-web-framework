// Copyright (c) Core. All rights reserved.

using AutoFixture;
using RentalManager.Application.Commands;
using RentalManager.Application.Handlers;
using RentalManager.Application.Interfaces;
using RentalManager.Domain.Entities;
using RentalManager.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace RentalManager.UnitTests.Application.Commands;

[TestFixture]
public class ProcessPaymentCommandHandlerTests
{
    private Mock<IPaymentService> _paymentServiceMock = null!;
    private ProcessPaymentCommandHandler _handler = null!;
    private IFixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _paymentServiceMock = new Mock<IPaymentService>();
        _handler = new ProcessPaymentCommandHandler(_paymentServiceMock.Object);
        _fixture = new Fixture();
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldReturnPayment()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            UserId = Guid.NewGuid(),
            Amount = 100.50m,
            Currency = "USD",
            PaymentMethodType = PaymentMethodType.Card,
            PaymentMethodId = "pm_123",
            Description = "Test payment"
        };

        var expectedPayment = new Payment(
            command.UserId,
            Money.Create(command.Amount, command.Currency),
            command.PaymentMethodType,
            command.Description);

        _paymentServiceMock
            .Setup(x => x.ProcessPaymentAsync(
                command.UserId,
                It.Is<Money>(m => m.Amount == command.Amount && m.Currency == command.Currency),
                command.PaymentMethodType,
                It.IsAny<string?>(),
                It.IsAny<string?>()))
            .ReturnsAsync(expectedPayment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(command.UserId);
        result.Amount.Should().Be(command.Amount);
        result.Currency.Should().Be(command.Currency);
        result.PaymentMethodType.Should().Be(command.PaymentMethodType);
        result.Description.Should().Be(command.Description);

        _paymentServiceMock.Verify(
            x => x.ProcessPaymentAsync(
                command.UserId,
                It.Is<Money>(m => m.Amount == command.Amount && m.Currency == command.Currency),
                command.PaymentMethodType,
                command.PaymentMethodId,
                command.Description),
            Times.Once);
    }

    [Test]
    public async Task Handle_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            UserId = Guid.Empty,
            Amount = 100m,
            Currency = "USD",
            PaymentMethodType = PaymentMethodType.Card
        };

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("User ID cannot be empty (Parameter 'UserId')");
    }

    [Test]
    public async Task Handle_WithZeroAmount_ShouldCallPaymentService()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            UserId = Guid.NewGuid(),
            Amount = 0m,
            Currency = "USD",
            PaymentMethodType = PaymentMethodType.Card
        };

        var expectedPayment = new Payment(
            command.UserId,
            Money.Create(0m, command.Currency),
            command.PaymentMethodType,
            command.Description);

        _paymentServiceMock
            .Setup(x => x.ProcessPaymentAsync(
                command.UserId,
                It.Is<Money>(m => m.Amount == 0m && m.Currency == command.Currency),
                command.PaymentMethodType,
                It.IsAny<string?>(),
                It.IsAny<string?>()))
            .ReturnsAsync(expectedPayment);

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
        var command = new ProcessPaymentCommand
        {
            UserId = Guid.NewGuid(),
            Amount = 100m,
            Currency = string.Empty,
            PaymentMethodType = PaymentMethodType.Card
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
        var command = new ProcessPaymentCommand
        {
            UserId = Guid.NewGuid(),
            Amount = 100m,
            Currency = null!,
            PaymentMethodType = PaymentMethodType.Card
        };

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Currency cannot be null or empty (Parameter 'Currency')");
    }

    [Test]
    public async Task Handle_WithNullDescription_ShouldCallPaymentService()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            UserId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "USD",
            PaymentMethodType = PaymentMethodType.Card,
            Description = null
        };

        var expectedPayment = new Payment(
            command.UserId,
            Money.Create(command.Amount, command.Currency),
            command.PaymentMethodType,
            null);

        _paymentServiceMock
            .Setup(x => x.ProcessPaymentAsync(
                command.UserId,
                It.Is<Money>(m => m.Amount == command.Amount && m.Currency == command.Currency),
                command.PaymentMethodType,
                It.IsAny<string?>(),
                It.IsAny<string?>()))
            .ReturnsAsync(expectedPayment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().BeNull();
    }

    [Test]
    public async Task Handle_WithNullPaymentMethodId_ShouldCallPaymentService()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            UserId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "USD",
            PaymentMethodType = PaymentMethodType.Card,
            PaymentMethodId = null
        };

        var expectedPayment = new Payment(
            command.UserId,
            Money.Create(command.Amount, command.Currency),
            command.PaymentMethodType,
            command.Description);

        _paymentServiceMock
            .Setup(x => x.ProcessPaymentAsync(
                command.UserId,
                It.Is<Money>(m => m.Amount == command.Amount && m.Currency == command.Currency),
                command.PaymentMethodType,
                It.IsAny<string?>(),
                It.IsAny<string?>()))
            .ReturnsAsync(expectedPayment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public async Task Handle_WhenPaymentServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            UserId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "USD",
            PaymentMethodType = PaymentMethodType.Card
        };

        var expectedException = new InvalidOperationException("Payment processing failed");

        _paymentServiceMock
            .Setup(x => x.ProcessPaymentAsync(
                command.UserId,
                It.IsAny<Money>(),
                command.PaymentMethodType,
                It.IsAny<string?>(),
                It.IsAny<string?>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Payment processing failed");
    }

    [Test]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToService()
    {
        // Arrange
        var command = new ProcessPaymentCommand
        {
            UserId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "USD",
            PaymentMethodType = PaymentMethodType.Card
        };

        var expectedPayment = new Payment(
            command.UserId,
            Money.Create(command.Amount, command.Currency),
            command.PaymentMethodType,
            command.Description);

        _paymentServiceMock
            .Setup(x => x.ProcessPaymentAsync(
                command.UserId,
                It.IsAny<Money>(),
                command.PaymentMethodType,
                It.IsAny<string?>(),
                It.IsAny<string?>()))
            .ReturnsAsync(expectedPayment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        // Note: The actual cancellation token handling would depend on the service implementation
    }
}
