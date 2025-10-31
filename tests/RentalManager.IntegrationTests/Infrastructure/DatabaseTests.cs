// <copyright file="DatabaseTests.cs" company="Core">
// Copyright (c) Core. All rights reserved.
// </copyright>

using FluentAssertions;
using RentalManager.Domain.Entities;
using RentalManager.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace RentalManager.IntegrationTests.Infrastructure;

/// <summary>
/// Integration tests for database operations.
/// </summary>
[TestFixture]
public class DatabaseTests : IntegrationTestBase
{
    /// <summary>
    /// Tests that the database can be accessed and is empty after setup.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Database_ShouldBeAccessible_AndEmpty()
    {
        // Arrange & Act
        var payments = await DbContext!.Payments.ToListAsync();
        var subscriptions = await DbContext.Subscriptions.ToListAsync();
        var paymentMethods = await DbContext.PaymentMethods.ToListAsync();

        // Assert
        payments.Should().BeEmpty();
        subscriptions.Should().BeEmpty();
        paymentMethods.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that a payment can be created and persisted to the database.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Database_ShouldPersistPayment()
    {
        // Arrange
        var amount = Money.Create(100.50m, "USD");
        var payment = new Payment(
            Guid.NewGuid(),
            amount,
            PaymentMethodType.Card,
            "Test Payment");

        // Act
        DbContext!.Payments.Add(payment);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedPayment = await DbContext.Payments.FirstOrDefaultAsync(p => p.Id == payment.Id);
        savedPayment.Should().NotBeNull();
        savedPayment!.Amount.Amount.Should().Be(100.50m);
        savedPayment.Amount.Currency.Should().Be("USD");
        savedPayment.Description.Should().Be("Test Payment");
        savedPayment.Status.Should().Be(PaymentStatus.Pending);
    }

    /// <summary>
    /// Tests that multiple payments can be queried efficiently.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Database_ShouldQueryMultiplePayments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount1 = Money.Create(50m, "USD");
        var amount2 = Money.Create(100m, "USD");
        var amount3 = Money.Create(150m, "EUR");

        var payment1 = new Payment(userId, amount1, PaymentMethodType.Card, "Payment 1");
        var payment2 = new Payment(userId, amount2, PaymentMethodType.Card, "Payment 2");
        var payment3 = new Payment(userId, amount3, PaymentMethodType.Card, "Payment 3");

        payment2.Succeed();

        DbContext!.Payments.AddRange(payment1, payment2, payment3);
        await DbContext.SaveChangesAsync();

        // Act
        var pendingPayments = await DbContext.Payments
            .Where(p => p.UserId == userId && p.Status == PaymentStatus.Pending)
            .ToListAsync();

        var succeededPayments = await DbContext.Payments
            .Where(p => p.UserId == userId && p.Status == PaymentStatus.Succeeded)
            .ToListAsync();

        // Assert
        pendingPayments.Should().HaveCount(2);
        succeededPayments.Should().HaveCount(1);
        succeededPayments.First().Description.Should().Be("Payment 2");
    }

    /// <summary>
    /// Tests that a subscription can be created and persisted.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Database_ShouldPersistSubscription()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = Money.Create(29.99m, "USD");
        var subscription = new Subscription(
            userId,
            "plan_premium",
            amount,
            "cus_test123");

        subscription.Activate(DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));

        // Act
        DbContext!.Subscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedSubscription = await DbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscription.Id);

        savedSubscription.Should().NotBeNull();
        savedSubscription!.UserId.Should().Be(userId);
        savedSubscription.PlanId.Should().Be("plan_premium");
        savedSubscription.Amount.Amount.Should().Be(29.99m);
        savedSubscription.Status.Should().Be(SubscriptionStatus.Active);
    }

    /// <summary>
    /// Tests that a payment method can be created and persisted.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Database_ShouldPersistPaymentMethod()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var paymentMethod = new PaymentMethod(
            userId,
            PaymentMethodType.Card,
            "pm_test123",
            "4242",
            "Visa",
            null,
            true);

        // Act
        DbContext!.PaymentMethods.Add(paymentMethod);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedMethod = await DbContext.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Id == paymentMethod.Id);

        savedMethod.Should().NotBeNull();
        savedMethod!.UserId.Should().Be(userId);
        savedMethod.StripePaymentMethodId.Should().Be("pm_test123");
        savedMethod.Type.Should().Be(PaymentMethodType.Card);
        savedMethod.IsDefault.Should().BeTrue();
        savedMethod.LastFourDigits.Should().Be("4242");
        savedMethod.Brand.Should().Be("Visa");
    }

    /// <summary>
    /// Tests that database transactions work correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Database_ShouldRollbackTransaction_OnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = Money.Create(100m, "USD");
        var payment = new Payment(userId, amount, PaymentMethodType.Card, "Test Payment");

        // Act
        using (var transaction = await DbContext!.Database.BeginTransactionAsync())
        {
            DbContext.Payments.Add(payment);
            await DbContext.SaveChangesAsync();

            // Verify payment is in the context
            var paymentInTransaction = await DbContext.Payments
                .FirstOrDefaultAsync(p => p.Id == payment.Id);
            paymentInTransaction.Should().NotBeNull();

            // Rollback
            await transaction.RollbackAsync();
        }

        // Assert - payment should not be persisted
        var rolledBackPayment = await DbContext.Payments
            .FirstOrDefaultAsync(p => p.Id == payment.Id);
        rolledBackPayment.Should().BeNull();
    }

    /// <summary>
    /// Tests that database cleanup works between tests.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Database_ShouldBeCleanBetweenTests()
    {
        // This test should always find an empty database due to Respawner cleanup
        var paymentCount = await DbContext!.Payments.CountAsync();
        var subscriptionCount = await DbContext.Subscriptions.CountAsync();

        paymentCount.Should().Be(0);
        subscriptionCount.Should().Be(0);
    }
}
