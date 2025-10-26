// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RentalManager.Application.Interfaces;

public interface IApplicationNotificationJobs
{
    Task SendApplicationSubmittedEmailAsync(Guid applicationId);

    Task SendApplicationFeePaymentConfirmationAsync(Guid applicationId);

    Task SendApplicationStatusUpdateEmailAsync(Guid applicationId, string status);

    Task SendPropertyOwnerNewApplicationNotificationAsync(Guid applicationId);
}

