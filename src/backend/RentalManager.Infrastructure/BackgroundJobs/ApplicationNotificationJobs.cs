// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Hangfire;
using Microsoft.Extensions.Logging;
using RentalManager.Application.Interfaces;
using RentalManager.Infrastructure.Services;

namespace RentalManager.Infrastructure.BackgroundJobs;

public class ApplicationNotificationJobs : IApplicationNotificationJobs
{
    private readonly EmailService _emailService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ApplicationNotificationJobs> _logger;

    public ApplicationNotificationJobs(
        EmailService emailService,
        IApplicationDbContext context,
        ILogger<ApplicationNotificationJobs> logger)
    {
        _emailService = emailService;
        _context = context;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task SendApplicationSubmittedEmailAsync(Guid applicationId)
    {
        try
        {
            var application = await _context.PropertyApplications.FindAsync(applicationId);
            if (application == null)
            {
                _logger.LogWarning("Application {ApplicationId} not found", applicationId);
                return;
            }

            var applicantEmail = ExtractEmailFromApplicationData(application.ApplicationData);
            if (string.IsNullOrEmpty(applicantEmail))
            {
                _logger.LogWarning("No email found for application {ApplicationId}", applicationId);
                return;
            }

            await _emailService.SendEmail(
                applicantEmail,
                "Application Submitted Successfully",
                $@"<h2>Your rental application has been submitted!</h2>
                   <p>Thank you for submitting your rental application.</p>
                   <p><strong>Application ID:</strong> {application.Id}</p>
                   <p><strong>Property ID:</strong> {application.PropertyId}</p>
                   <p><strong>Application Fee:</strong> {application.ApplicationFee.Currency} {application.ApplicationFee.Amount:F2}</p>
                   <p>We will review your application and get back to you soon.</p>
                   <p>If you have any questions, please don't hesitate to contact us.</p>");

            _logger.LogInformation("Sent application submitted email for application {ApplicationId}", applicationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending application submitted email for {ApplicationId}", applicationId);
            throw;
        }
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task SendApplicationFeePaymentConfirmationAsync(Guid applicationId)
    {
        try
        {
            var application = await _context.PropertyApplications.FindAsync(applicationId);
            if (application == null)
            {
                _logger.LogWarning("Application {ApplicationId} not found", applicationId);
                return;
            }

            var applicantEmail = ExtractEmailFromApplicationData(application.ApplicationData);
            if (string.IsNullOrEmpty(applicantEmail))
            {
                _logger.LogWarning("No email found for application {ApplicationId}", applicationId);
                return;
            }

            await _emailService.SendEmail(
                applicantEmail,
                "Application Fee Payment Received",
                $@"<h2>Payment Confirmation</h2>
                   <p>We have received your application fee payment.</p>
                   <p><strong>Amount Paid:</strong> {application.ApplicationFee.Currency} {application.ApplicationFee.Amount:F2}</p>
                   <p><strong>Application ID:</strong> {application.Id}</p>
                   <p><strong>Payment ID:</strong> {application.ApplicationFeePaymentId}</p>
                   <p>Your application is now under review. We will notify you of any updates.</p>
                   <p>Thank you for your payment!</p>");

            _logger.LogInformation("Sent payment confirmation email for application {ApplicationId}", applicationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment confirmation email for {ApplicationId}", applicationId);
            throw;
        }
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task SendApplicationStatusUpdateEmailAsync(Guid applicationId, string status)
    {
        try
        {
            var application = await _context.PropertyApplications.FindAsync(applicationId);
            if (application == null)
            {
                _logger.LogWarning("Application {ApplicationId} not found", applicationId);
                return;
            }

            var applicantEmail = ExtractEmailFromApplicationData(application.ApplicationData);
            if (string.IsNullOrEmpty(applicantEmail))
            {
                _logger.LogWarning("No email found for application {ApplicationId}", applicationId);
                return;
            }

            var subject = status == "Approved"
                ? "Your Rental Application Has Been Approved!"
                : "Update on Your Rental Application";

            var message = status == "Approved"
                ? $@"<h2>Congratulations!</h2>
                     <p>Your rental application has been approved!</p>
                     <p><strong>Application ID:</strong> {application.Id}</p>
                     <p><strong>Property ID:</strong> {application.PropertyId}</p>
                     {(string.IsNullOrEmpty(application.DecisionNotes) ? string.Empty : $"<p><strong>Notes:</strong> {application.DecisionNotes}</p>")}
                     <p>We will contact you shortly with the next steps for signing your lease.</p>"
                : $@"<h2>Application Status Update</h2>
                     <p>Your rental application status has been updated to: <strong>{status}</strong></p>
                     <p><strong>Application ID:</strong> {application.Id}</p>
                     <p><strong>Property ID:</strong> {application.PropertyId}</p>
                     {(string.IsNullOrEmpty(application.DecisionNotes) ? string.Empty : $"<p><strong>Notes:</strong> {application.DecisionNotes}</p>")}
                     <p>If you have any questions, please contact us.</p>";

            await _emailService.SendEmail(applicantEmail, subject, message);

            _logger.LogInformation("Sent status update email for application {ApplicationId}", applicationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending status update email for {ApplicationId}", applicationId);
            throw;
        }
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task SendPropertyOwnerNewApplicationNotificationAsync(Guid applicationId)
    {
        try
        {
            var application = await _context.PropertyApplications.FindAsync(applicationId);
            if (application == null)
            {
                _logger.LogWarning("Application {ApplicationId} not found", applicationId);
                return;
            }

            var property = await _context.Properties.FindAsync(application.PropertyId);
            if (property == null)
            {
                _logger.LogWarning("Property {PropertyId} not found", application.PropertyId);
                return;
            }

            // Get owner email (in a real system, you'd fetch this from the User table)
            // For now, we'll log it
            _logger.LogInformation("New application {ApplicationId} received for property {PropertyId}", applicationId, property.Id);

            // TODO: Fetch owner email and send notification
            // await _emailService.SendEmailAsync(ownerEmail, "New Rental Application", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending owner notification for {ApplicationId}", applicationId);
            throw;
        }
    }

    private static string? ExtractEmailFromApplicationData(string applicationDataJson)
    {
        try
        {
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(applicationDataJson);
            if (data != null && data.TryGetValue("email", out var email))
            {
                return email.ToString();
            }
        }
        catch
        {
            // Ignore JSON parsing errors
        }

        return null;
    }
}

