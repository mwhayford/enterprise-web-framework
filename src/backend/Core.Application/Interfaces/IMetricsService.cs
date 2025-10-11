// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace Core.Application.Interfaces;

public interface IMetricsService
{
    void RecordUserRegistration();
    void RecordPaymentProcessed(string status);
    void RecordSubscriptionCreated(string planId);
    void RecordEmailSent(string emailType);
    void RecordPaymentProcessingTime(TimeSpan duration);
    void RecordUserRegistrationTime(TimeSpan duration);
}
