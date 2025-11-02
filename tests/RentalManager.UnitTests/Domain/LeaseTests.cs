// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using RentalManager.Domain.Entities;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.UnitTests.Domain;

public class LeaseTests
{
    [Test]
    public void Lease_Creation_Should_Set_Properties_Correctly()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var landlordId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddMonths(12);
        var monthlyRent = Money.Create(1500, "USD");
        var securityDeposit = Money.Create(1500, "USD");

        // Act
        var lease = new Lease(
            propertyId,
            tenantId,
            landlordId,
            startDate,
            endDate,
            monthlyRent,
            securityDeposit,
            PaymentFrequency.Monthly,
            1,
            "No pets allowed");

        // Assert
        Assert.That(lease.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(lease.PropertyId, Is.EqualTo(propertyId));
        Assert.That(lease.TenantId, Is.EqualTo(tenantId));
        Assert.That(lease.LandlordId, Is.EqualTo(landlordId));
        Assert.That(lease.StartDate, Is.EqualTo(startDate));
        Assert.That(lease.EndDate, Is.EqualTo(endDate));
        Assert.That(lease.MonthlyRent.Amount, Is.EqualTo(1500));
        Assert.That(lease.MonthlyRent.Currency, Is.EqualTo("USD"));
        Assert.That(lease.SecurityDeposit.Amount, Is.EqualTo(1500));
        Assert.That(lease.PaymentFrequency, Is.EqualTo(PaymentFrequency.Monthly));
        Assert.That(lease.PaymentDayOfMonth, Is.EqualTo(1));
        Assert.That(lease.Status, Is.EqualTo(LeaseStatus.Draft));
        Assert.That(lease.SpecialTerms, Is.EqualTo("No pets allowed"));
    }

    [Test]
    public void Lease_Creation_Should_Fail_With_Invalid_PropertyId()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var tenantId = Guid.NewGuid();
        var landlordId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddMonths(12);
        var monthlyRent = Money.Create(1500, "USD");
        var securityDeposit = Money.Create(1500, "USD");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Lease(
            emptyGuid,
            tenantId,
            landlordId,
            startDate,
            endDate,
            monthlyRent,
            securityDeposit,
            PaymentFrequency.Monthly,
            1));

        Assert.That(exception.Message, Does.Contain("Property ID cannot be empty"));
    }

    [Test]
    public void Lease_Creation_Should_Fail_When_EndDate_Before_StartDate()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var landlordId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(-1); // End date before start date
        var monthlyRent = Money.Create(1500, "USD");
        var securityDeposit = Money.Create(1500, "USD");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Lease(
            propertyId,
            tenantId,
            landlordId,
            startDate,
            endDate,
            monthlyRent,
            securityDeposit,
            PaymentFrequency.Monthly,
            1));

        Assert.That(exception.Message, Does.Contain("End date must be after start date"));
    }

    [Test]
    public void Activate_Should_Change_Status_To_Active()
    {
        // Arrange
        var lease = CreateTestLease();

        // Act
        lease.Activate();

        // Assert
        Assert.That(lease.Status, Is.EqualTo(LeaseStatus.Active));
        Assert.That(lease.ActivatedAt, Is.Not.Null);
        Assert.That(lease.IsActive, Is.True);
    }

    [Test]
    public void Terminate_Should_Change_Status_To_Terminated()
    {
        // Arrange
        var lease = CreateTestLease();
        lease.Activate();
        var reason = "Tenant requested early termination";

        // Act
        lease.Terminate(reason);

        // Assert
        Assert.That(lease.Status, Is.EqualTo(LeaseStatus.Terminated));
        Assert.That(lease.TerminatedAt, Is.Not.Null);
        Assert.That(lease.TerminationReason, Is.EqualTo(reason));
    }

    [Test]
    public void Terminate_Should_Fail_With_Empty_Reason()
    {
        // Arrange
        var lease = CreateTestLease();
        lease.Activate();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => lease.Terminate(string.Empty));
        Assert.That(exception.Message, Does.Contain("Termination reason is required"));
    }

    [Test]
    public void Terminate_Should_Fail_When_Lease_Not_Active()
    {
        // Arrange
        var lease = CreateTestLease(); // Draft status

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            lease.Terminate("Some reason"));

        Assert.That(exception.Message, Does.Contain("cannot be terminated"));
    }

    [Test]
    public void MarkAsExpired_Should_Change_Status_To_Expired()
    {
        // Arrange
        var lease = CreateTestLease();
        lease.Activate();

        // Act
        lease.MarkAsExpired();

        // Assert
        Assert.That(lease.Status, Is.EqualTo(LeaseStatus.Expired));
    }

    [Test]
    public void Renew_Should_Create_New_Lease_And_Mark_Current_As_Renewed()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-365); // One year ago
        var endDate = DateTime.UtcNow.AddDays(-30); // Expired 30 days ago
        var lease = CreateTestLease(startDate, endDate);
        lease.Activate();
        lease.MarkAsExpired(); // Mark as expired so it can be renewed
        var newEndDate = endDate.AddYears(1);

        // Act
        var renewedLease = lease.Renew(newEndDate);

        // Assert
        Assert.That(lease.Status, Is.EqualTo(LeaseStatus.Renewed));
        Assert.That(renewedLease.Id, Is.Not.EqualTo(lease.Id));
        Assert.That(renewedLease.PropertyId, Is.EqualTo(lease.PropertyId));
        Assert.That(renewedLease.TenantId, Is.EqualTo(lease.TenantId));
        Assert.That(renewedLease.EndDate, Is.EqualTo(newEndDate));
        Assert.That(renewedLease.Status, Is.EqualTo(LeaseStatus.Draft));
    }

    [Test]
    public void Renew_Should_Update_Monthly_Rent_If_Provided()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-365); // One year ago
        var endDate = DateTime.UtcNow.AddDays(-30); // Expired 30 days ago
        var lease = CreateTestLease(startDate, endDate);
        lease.Activate();
        lease.MarkAsExpired(); // Mark as expired so it can be renewed
        var newEndDate = endDate.AddYears(1);
        var newRent = Money.Create(1600, "USD");

        // Act
        var renewedLease = lease.Renew(newEndDate, newRent);

        // Assert
        Assert.That(renewedLease.MonthlyRent.Amount, Is.EqualTo(1600));
    }

    [Test]
    public void UpdateRent_Should_Change_Monthly_Rent()
    {
        // Arrange
        var lease = CreateTestLease();
        var newRent = Money.Create(1750, "USD");

        // Act
        lease.UpdateRent(newRent);

        // Assert
        Assert.That(lease.MonthlyRent.Amount, Is.EqualTo(1750));
    }

    [Test]
    public void GetDurationInDays_Should_Return_Correct_Duration()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2026, 1, 1);
        var lease = CreateTestLease(startDate, endDate);

        // Act
        var duration = lease.GetDurationInDays();

        // Assert
        Assert.That(duration, Is.EqualTo(365)); // One year duration
    }

    [Test]
    public void AttachApplication_Should_Set_PropertyApplicationId()
    {
        // Arrange
        var lease = CreateTestLease();
        var applicationId = Guid.NewGuid();

        // Act
        lease.AttachApplication(applicationId);

        // Assert
        Assert.That(lease.PropertyApplicationId, Is.EqualTo(applicationId));
    }

    [Test]
    public void UpdateSpecialTerms_Should_Update_Terms()
    {
        // Arrange
        var lease = CreateTestLease();
        var newTerms = "Pets allowed with $500 deposit";

        // Act
        lease.UpdateSpecialTerms(newTerms);

        // Assert
        Assert.That(lease.SpecialTerms, Is.EqualTo(newTerms));
    }

    private static Lease CreateTestLease(DateTime? startDate = null, DateTime? endDate = null)
    {
        var propertyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var landlordId = Guid.NewGuid();
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? start.AddMonths(12);
        var monthlyRent = Money.Create(1500, "USD");
        var securityDeposit = Money.Create(1500, "USD");

        return new Lease(
            propertyId,
            tenantId,
            landlordId,
            start,
            end,
            monthlyRent,
            securityDeposit,
            PaymentFrequency.Monthly,
            1,
            "No pets allowed");
    }
}
