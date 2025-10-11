using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Core.Application.Interfaces;

namespace Core.Infrastructure.Services;

public class MetricsService : IMetricsService
{
    private readonly Meter _meter;
    private readonly Counter<long> _userRegistrations;
    private readonly Counter<long> _paymentsProcessed;
    private readonly Counter<long> _subscriptionsCreated;
    private readonly Counter<long> _emailsSent;
    private readonly Histogram<double> _paymentProcessingTime;
    private readonly Histogram<double> _userRegistrationTime;
    private readonly ILogger<MetricsService> _logger;

    public MetricsService(ILogger<MetricsService> logger)
    {
        _logger = logger;
        _meter = new Meter("Core.API", "1.0.0");
        
        _userRegistrations = _meter.CreateCounter<long>(
            "core_user_registrations_total",
            "Total number of user registrations");
            
        _paymentsProcessed = _meter.CreateCounter<long>(
            "core_payments_processed_total",
            "Total number of payments processed");
            
        _subscriptionsCreated = _meter.CreateCounter<long>(
            "core_subscriptions_created_total",
            "Total number of subscriptions created");
            
        _emailsSent = _meter.CreateCounter<long>(
            "core_emails_sent_total",
            "Total number of emails sent");
            
        _paymentProcessingTime = _meter.CreateHistogram<double>(
            "core_payment_processing_duration_seconds",
            "Time spent processing payments");
            
        _userRegistrationTime = _meter.CreateHistogram<double>(
            "core_user_registration_duration_seconds",
            "Time spent registering users");
    }

    public void RecordUserRegistration()
    {
        _userRegistrations.Add(1);
        _logger.LogDebug("Recorded user registration metric");
    }

    public void RecordPaymentProcessed(string status)
    {
        _paymentsProcessed.Add(1, new KeyValuePair<string, object?>("status", status));
        _logger.LogDebug("Recorded payment processed metric with status {Status}", status);
    }

    public void RecordSubscriptionCreated(string planId)
    {
        _subscriptionsCreated.Add(1, new KeyValuePair<string, object?>("plan_id", planId));
        _logger.LogDebug("Recorded subscription created metric for plan {PlanId}", planId);
    }

    public void RecordEmailSent(string emailType)
    {
        _emailsSent.Add(1, new KeyValuePair<string, object?>("email_type", emailType));
        _logger.LogDebug("Recorded email sent metric for type {EmailType}", emailType);
    }

    public void RecordPaymentProcessingTime(TimeSpan duration)
    {
        _paymentProcessingTime.Record(duration.TotalSeconds);
        _logger.LogDebug("Recorded payment processing time: {Duration}ms", duration.TotalMilliseconds);
    }

    public void RecordUserRegistrationTime(TimeSpan duration)
    {
        _userRegistrationTime.Record(duration.TotalSeconds);
        _logger.LogDebug("Recorded user registration time: {Duration}ms", duration.TotalMilliseconds);
    }

    public void Dispose()
    {
        _meter?.Dispose();
    }
}
