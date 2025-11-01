// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace RentalManager.Application.DTOs;

public class ApplicationSettingsDto
{
    public Guid Id { get; set; }

    public decimal DefaultApplicationFee { get; set; }

    public string DefaultApplicationFeeCurrency { get; set; } = default!;

    public bool ApplicationFeeEnabled { get; set; }

    public bool RequirePaymentUpfront { get; set; }

    public int? MaxApplicationsPerUser { get; set; }

    public string? ApplicationFormFields { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime UpdatedAt { get; set; }
}
