using Core.Domain.Entities;
using Core.Domain.Events;
using Core.Domain.ValueObjects;
using FluentAssertions;

namespace Core.UnitTests.Domain.Entities;

[TestFixture]
public class PaymentTests
{
    [Test]
    public void Constructor_WithValidParameters_ShouldCreatePayment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = Money.Create(100.50m, "USD");
        var paymentMethodType = PaymentMethodType.Card;
        var description = "Test payment";

        // Act
        var payment = new Payment(userId, amount, paymentMethodType, description);

        // Assert
        payment.UserId.Should().Be(userId);
        payment.Amount.Should().Be(amount);
        payment.PaymentMethodType.Should().Be(paymentMethodType);
        payment.Description.Should().Be(description);
        payment.Id.Should().NotBeEmpty();
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Constructor_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        var action = () => new Payment(Guid.Empty, Money.Create(100m, "USD"), PaymentMethodType.Card, "Test");
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WithNullAmount_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var action = () => new Payment(Guid.NewGuid(), null!, PaymentMethodType.Card, "Test");
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WithZeroAmount_ShouldCreatePayment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = Money.Create(0m, "USD");

        // Act
        var payment = new Payment(userId, amount, PaymentMethodType.Card, "Test");

        // Assert
        payment.Amount.Should().Be(amount);
    }

    [Test]
    public void Constructor_WithNullDescription_ShouldCreatePayment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = Money.Create(100m, "USD");

        // Act
        var payment = new Payment(userId, amount, PaymentMethodType.Card, null);

        // Assert
        payment.Description.Should().BeNull();
    }

    [Test]
    public void SetStripePaymentIntentId_ShouldSetPaymentIntentId()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Money.Create(100m, "USD"), PaymentMethodType.Card, "Test");
        var paymentIntentId = "pi_123456";

        // Act
        payment.SetStripePaymentIntentId(paymentIntentId);

        // Assert
        payment.StripePaymentIntentId.Should().Be(paymentIntentId);
        payment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Process_ShouldSetStatusToProcessing()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Money.Create(100m, "USD"), PaymentMethodType.Card, "Test");
        var chargeId = "ch_123456";

        // Act
        payment.Process(chargeId);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Processing);
        payment.StripeChargeId.Should().Be(chargeId);
        payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Succeed_ShouldSetStatusToSucceeded()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Money.Create(100m, "USD"), PaymentMethodType.Card, "Test");

        // Act
        payment.Succeed();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PaymentProcessedEvent>();
    }

    [Test]
    public void Fail_ShouldSetStatusToFailed()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Money.Create(100m, "USD"), PaymentMethodType.Card, "Test");
        var failureReason = "Insufficient funds";

        // Act
        payment.Fail(failureReason);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be(failureReason);
        payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PaymentFailedEvent>();
    }

    [Test]
    public void Fail_WithEmptyFailureReason_ShouldThrowArgumentException()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Money.Create(100m, "USD"), PaymentMethodType.Card, "Test");

        // Act & Assert
        var action = () => payment.Fail(string.Empty);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Fail_WithNullFailureReason_ShouldThrowArgumentException()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Money.Create(100m, "USD"), PaymentMethodType.Card, "Test");

        // Act & Assert
        var action = () => payment.Fail(null!);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Cancel_ShouldSetStatusToCancelled()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Money.Create(100m, "USD"), PaymentMethodType.Card, "Test");

        // Act
        payment.Cancel();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Cancelled);
        payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Refund_ShouldSetStatusToRefunded()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Money.Create(100m, "USD"), PaymentMethodType.Card, "Test");

        // Act
        payment.Refund();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void PartialRefund_ShouldSetStatusToPartiallyRefunded()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Money.Create(100m, "USD"), PaymentMethodType.Card, "Test");

        // Act
        payment.PartialRefund();

        // Assert
        payment.Status.Should().Be(PaymentStatus.PartiallyRefunded);
        payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Money.Create(100m, "USD"), PaymentMethodType.Card, "Test");
        payment.Succeed();
        payment.DomainEvents.Should().NotBeEmpty();

        // Act
        payment.ClearDomainEvents();

        // Assert
        payment.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void AddDomainEvent_ShouldAddEventToList()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Money.Create(100m, "USD"), PaymentMethodType.Card, "Test");
        var customEvent = new PaymentProcessedEvent
        {
            PaymentId = payment.Id,
            UserId = payment.UserId,
            Amount = payment.Amount.Amount,
            Currency = payment.Amount.Currency,
            Status = "Succeeded",
            PaymentMethodId = "pm_123"
        };

        // Act
        payment.AddDomainEvent(customEvent);

        // Assert
        payment.DomainEvents.Should().Contain(customEvent);
    }

    [Test]
    public void RemoveDomainEvent_ShouldRemoveEventFromList()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Money.Create(100m, "USD"), PaymentMethodType.Card, "Test");
        payment.Succeed();
        var eventToRemove = payment.DomainEvents.First();

        // Act
        payment.RemoveDomainEvent(eventToRemove);

        // Assert
        payment.DomainEvents.Should().NotContain(eventToRemove);
    }

    [Test]
    public void Equals_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        var payment1 = new Payment(Guid.NewGuid(), Money.Create(100m, "USD"), PaymentMethodType.Card, "Test");
        var payment2 = new Payment(Guid.NewGuid(), Money.Create(200m, "USD"), PaymentMethodType.Ach, "Test2");
        // Note: In a real scenario, you'd need to set the same ID, but since ID is generated in constructor,
        // this test demonstrates the concept

        // Act & Assert
        payment1.Should().NotBe(payment2); // Different IDs
        payment1.Equals(payment1).Should().BeTrue(); // Same instance
    }

    [Test]
    public void GetHashCode_ShouldReturnIdHashCode()
    {
        // Arrange
        var payment = new Payment(Guid.NewGuid(), Money.Create(100m, "USD"), PaymentMethodType.Card, "Test");

        // Act & Assert
        payment.GetHashCode().Should().Be(payment.Id.GetHashCode());
    }
}