// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.Extensions.Logging;
using RentalManager.Application.Interfaces;

namespace RentalManager.Infrastructure.Services;

public class EmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly IMetricsService _metricsService;

    public EmailService(ILogger<EmailService> logger, IBackgroundJobService backgroundJobService, IMetricsService metricsService)
    {
        _logger = logger;
        _backgroundJobService = backgroundJobService;
        _metricsService = metricsService;
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName)
    {
        _logger.LogInformation("Sending welcome email to {Email}", email);

        // Simulate email sending
        await Task.Delay(1000);

        _metricsService.RecordEmailSent("welcome");
        _logger.LogInformation("Welcome email sent to {Email}", email);
    }

    public async Task SendPaymentConfirmationEmailAsync(string email, decimal amount, string currency)
    {
        _logger.LogInformation(
            "Sending payment confirmation email to {Email} for {Amount} {Currency}",
            email,
            amount,
            currency);

        // Simulate email sending
        await Task.Delay(1000);

        _metricsService.RecordEmailSent("payment_confirmation");
        _logger.LogInformation("Payment confirmation email sent to {Email}", email);
    }

    public async Task SendSubscriptionReminderEmailAsync(string email, string planName, DateTime renewalDate)
    {
        _logger.LogInformation(
            "Sending subscription reminder email to {Email} for plan {PlanName}",
            email,
            planName);

        // Simulate email sending
        await Task.Delay(1000);

        _logger.LogInformation("Subscription reminder email sent to {Email}", email);
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        _logger.LogInformation("Sending password reset email to {Email}", email);

        // Simulate email sending
        await Task.Delay(1000);

        _logger.LogInformation("Password reset email sent to {Email}", email);
    }

    public async Task SendEmail(string toEmail, string subject, string htmlBody)
    {
        _logger.LogInformation("Sending email to {Email} with subject {Subject}", toEmail, subject);

        // Simulate email sending
        await Task.Delay(500);

        _metricsService.RecordEmailSent("application");
        _logger.LogInformation("Email sent to {Email}", toEmail);
    }
}
