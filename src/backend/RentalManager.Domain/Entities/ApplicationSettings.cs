// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Domain.Entities;

public class ApplicationSettings : BaseEntity
{
    public ApplicationSettings(
        Money defaultApplicationFee,
        bool applicationFeeEnabled,
        bool requirePaymentUpfront,
        string? applicationFormFields = null,
        int? maxApplicationsPerUser = null)
        : base()
    {
        ArgumentNullException.ThrowIfNull(defaultApplicationFee);

        DefaultApplicationFee = defaultApplicationFee;
        ApplicationFeeEnabled = applicationFeeEnabled;
        RequirePaymentUpfront = requirePaymentUpfront;
        ApplicationFormFields = applicationFormFields;
        MaxApplicationsPerUser = maxApplicationsPerUser;
    }

    private ApplicationSettings()
    {
        DefaultApplicationFee = default!;
    } // For EF Core

    public Money DefaultApplicationFee { get; private set; }

    public bool ApplicationFeeEnabled { get; private set; }

    public bool RequirePaymentUpfront { get; private set; }

    public int? MaxApplicationsPerUser { get; private set; }

    public string? ApplicationFormFields { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public void UpdateFee(Money newFee, Guid updatedBy)
    {
        ArgumentNullException.ThrowIfNull(newFee);

        if (updatedBy == Guid.Empty)
        {
            throw new ArgumentException("UpdatedBy ID cannot be empty", nameof(updatedBy));
        }

        DefaultApplicationFee = newFee;
        UpdatedBy = updatedBy;
        UpdateTimestamp();
    }

    public void UpdateSettings(
        bool applicationFeeEnabled,
        bool requirePaymentUpfront,
        int? maxApplicationsPerUser,
        string? applicationFormFields,
        Guid updatedBy)
    {
        if (updatedBy == Guid.Empty)
        {
            throw new ArgumentException("UpdatedBy ID cannot be empty", nameof(updatedBy));
        }

        ApplicationFeeEnabled = applicationFeeEnabled;
        RequirePaymentUpfront = requirePaymentUpfront;
        MaxApplicationsPerUser = maxApplicationsPerUser;
        ApplicationFormFields = applicationFormFields;
        UpdatedBy = updatedBy;
        UpdateTimestamp();
    }

    public static ApplicationSettings CreateDefault()
    {
        return new ApplicationSettings(
            Money.Create(35.00m, "USD"),
            applicationFeeEnabled: true,
            requirePaymentUpfront: true,
            applicationFormFields: null,
            maxApplicationsPerUser: null);
    }
}

