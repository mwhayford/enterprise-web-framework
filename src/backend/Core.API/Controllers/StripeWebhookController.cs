// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Events;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

namespace Core.API.Controllers;

[ApiController]
[Route("api/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StripeWebhookController> _logger;
    private readonly IEventPublisher _eventPublisher;
    private readonly IMetricsService _metricsService;
    private readonly string _webhookSecret;

    public StripeWebhookController(
        ApplicationDbContext context,
        ILogger<StripeWebhookController> logger,
        IEventPublisher eventPublisher,
        IMetricsService metricsService,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _eventPublisher = eventPublisher;
        _metricsService = metricsService;
        _webhookSecret = configuration["StripeSettings:WebhookSecret"] 
            ?? throw new InvalidOperationException("Stripe webhook secret is not configured");
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        
        try
        {
            var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                signatureHeader,
                _webhookSecret
            );

            _logger.LogInformation("Received Stripe webhook event: {EventType} with ID: {EventId}", 
                stripeEvent.Type, stripeEvent.Id);

            // Handle the event based on its type
            switch (stripeEvent.Type)
            {
                case Events.PaymentIntentSucceeded:
                    await HandlePaymentIntentSucceeded(stripeEvent);
                    break;

                case Events.PaymentIntentPaymentFailed:
                    await HandlePaymentIntentFailed(stripeEvent);
                    break;

                case Events.ChargeSucceeded:
                    await HandleChargeSucceeded(stripeEvent);
                    break;

                case Events.ChargeFailed:
                    await HandleChargeFailed(stripeEvent);
                    break;

                case Events.InvoicePaid:
                    await HandleInvoicePaid(stripeEvent);
                    break;

                case Events.InvoicePaymentFailed:
                    await HandleInvoicePaymentFailed(stripeEvent);
                    break;

                case Events.CustomerSubscriptionCreated:
                    await HandleSubscriptionCreated(stripeEvent);
                    break;

                case Events.CustomerSubscriptionUpdated:
                    await HandleSubscriptionUpdated(stripeEvent);
                    break;

                case Events.CustomerSubscriptionDeleted:
                    await HandleSubscriptionDeleted(stripeEvent);
                    break;

                case Events.PaymentMethodAttached:
                    await HandlePaymentMethodAttached(stripeEvent);
                    break;

                default:
                    _logger.LogInformation("Unhandled Stripe webhook event type: {EventType}", stripeEvent.Type);
                    break;
            }

            // Record metrics
            _metricsService.RecordWebhookReceived(stripeEvent.Type);

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed");
            return BadRequest(new { error = "Invalid signature" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return StatusCode(500, new { error = "Webhook processing failed" });
        }
    }

    private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null) return;

        _logger.LogInformation("Payment intent succeeded: {PaymentIntentId}", paymentIntent.Id);

        // Find the payment by Stripe PaymentIntent ID
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.StripeChargeId == paymentIntent.Id);

        if (payment != null)
        {
            payment.Succeed();
            await _context.SaveChangesAsync();

            // Publish event
            await _eventPublisher.PublishAsync(new PaymentProcessedEvent
            {
                PaymentId = payment.Id,
                UserId = payment.UserId,
                Amount = payment.Amount.Amount,
                Currency = payment.Amount.Currency,
                Status = payment.Status.ToString(),
                PaymentMethodId = paymentIntent.PaymentMethodId ?? string.Empty
            });

            _logger.LogInformation("Payment {PaymentId} marked as succeeded", payment.Id);
        }
        else
        {
            _logger.LogWarning("Payment not found for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
        }
    }

    private async Task HandlePaymentIntentFailed(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null) return;

        _logger.LogWarning("Payment intent failed: {PaymentIntentId}", paymentIntent.Id);

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.StripeChargeId == paymentIntent.Id);

        if (payment != null)
        {
            var failureReason = paymentIntent.LastPaymentError?.Message ?? "Payment failed";
            payment.Fail(failureReason);
            await _context.SaveChangesAsync();

            // Publish event
            await _eventPublisher.PublishAsync(new PaymentFailedEvent
            {
                PaymentId = payment.Id,
                UserId = payment.UserId,
                Amount = payment.Amount.Amount,
                Currency = payment.Amount.Currency,
                FailureReason = failureReason
            });

            _logger.LogInformation("Payment {PaymentId} marked as failed: {FailureReason}", 
                payment.Id, failureReason);
        }
    }

    private async Task HandleChargeSucceeded(Event stripeEvent)
    {
        var charge = stripeEvent.Data.Object as Charge;
        if (charge == null) return;

        _logger.LogInformation("Charge succeeded: {ChargeId}", charge.Id);

        // Update payment with charge ID if it exists
        if (charge.PaymentIntentId != null)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripeChargeId == charge.PaymentIntentId);

            if (payment != null)
            {
                payment.SetStripePaymentIntentId(charge.Id);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Payment {PaymentId} updated with ChargeId: {ChargeId}", 
                    payment.Id, charge.Id);
            }
        }
    }

    private async Task HandleChargeFailed(Event stripeEvent)
    {
        var charge = stripeEvent.Data.Object as Charge;
        if (charge == null) return;

        _logger.LogWarning("Charge failed: {ChargeId}", charge.Id);

        if (charge.PaymentIntentId != null)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripeChargeId == charge.PaymentIntentId);

            if (payment != null)
            {
                var failureReason = charge.FailureMessage ?? "Charge failed";
                payment.Fail(failureReason);
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task HandleInvoicePaid(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null) return;

        _logger.LogInformation("Invoice paid: {InvoiceId} for subscription: {SubscriptionId}", 
            invoice.Id, invoice.SubscriptionId);

        if (invoice.SubscriptionId != null)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == invoice.SubscriptionId);

            if (subscription != null)
            {
                // Create a payment record for the subscription payment
                var amount = Money.Create(
                    invoice.AmountPaid / 100m, 
                    invoice.Currency.ToUpper()
                );

                var payment = new Payment(
                    subscription.UserId,
                    amount,
                    PaymentMethodType.Card,
                    $"Subscription payment for invoice {invoice.Id}"
                );
                payment.SetStripePaymentIntentId(invoice.PaymentIntentId ?? invoice.Id);
                payment.Succeed();

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created payment record {PaymentId} for subscription invoice", 
                    payment.Id);
            }
        }
    }

    private async Task HandleInvoicePaymentFailed(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null) return;

        _logger.LogWarning("Invoice payment failed: {InvoiceId} for subscription: {SubscriptionId}", 
            invoice.Id, invoice.SubscriptionId);

        if (invoice.SubscriptionId != null)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == invoice.SubscriptionId);

            if (subscription != null)
            {
                // You might want to handle failed subscription payments here
                // For example, send notifications or update subscription status
                _logger.LogWarning("Subscription {SubscriptionId} payment failed", subscription.Id);
            }
        }
    }

    private async Task HandleSubscriptionCreated(Event stripeEvent)
    {
        var stripeSubscription = stripeEvent.Data.Object as Subscription;
        if (stripeSubscription == null) return;

        _logger.LogInformation("Subscription created: {SubscriptionId}", stripeSubscription.Id);

        // The subscription should already exist from our API call
        // This webhook confirms it was created successfully in Stripe
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscription.Id);

        if (subscription != null && stripeSubscription.Status == "active")
        {
            subscription.Activate(
                DateTime.UtcNow,
                stripeSubscription.CurrentPeriodEnd
            );
            await _context.SaveChangesAsync();
        }
    }

    private async Task HandleSubscriptionUpdated(Event stripeEvent)
    {
        var stripeSubscription = stripeEvent.Data.Object as Subscription;
        if (stripeSubscription == null) return;

        _logger.LogInformation("Subscription updated: {SubscriptionId}, Status: {Status}", 
            stripeSubscription.Id, stripeSubscription.Status);

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscription.Id);

        if (subscription != null)
        {
            switch (stripeSubscription.Status)
            {
                case "active":
                    subscription.Activate(
                        DateTime.UtcNow,
                        stripeSubscription.CurrentPeriodEnd
                    );
                    break;

                case "trialing":
                    subscription.StartTrial(
                        DateTime.UtcNow,
                        stripeSubscription.TrialEnd ?? DateTime.UtcNow.AddDays(7)
                    );
                    break;

                case "canceled":
                case "unpaid":
                    subscription.Cancel(DateTime.UtcNow);
                    break;
            }

            await _context.SaveChangesAsync();
        }
    }

    private async Task HandleSubscriptionDeleted(Event stripeEvent)
    {
        var stripeSubscription = stripeEvent.Data.Object as Subscription;
        if (stripeSubscription == null) return;

        _logger.LogInformation("Subscription deleted: {SubscriptionId}", stripeSubscription.Id);

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscription.Id);

        if (subscription != null)
        {
            subscription.Cancel(DateTime.UtcNow);
            await _context.SaveChangesAsync();
        }
    }

    private async Task HandlePaymentMethodAttached(Event stripeEvent)
    {
        var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
        if (paymentMethod == null) return;

        _logger.LogInformation("Payment method attached: {PaymentMethodId} for customer: {CustomerId}", 
            paymentMethod.Id, paymentMethod.CustomerId);

        // You could store payment method information here if needed
        // For now, just log it
    }
}



