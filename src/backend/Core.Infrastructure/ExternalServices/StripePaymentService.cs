using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Stripe;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Core.Domain.Events;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Identity;
using StripeSubscription = Stripe.Subscription;

namespace Core.Infrastructure.ExternalServices;

public class StripePaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventPublisher _eventPublisher;

    public StripePaymentService(ApplicationDbContext context, ILogger<StripePaymentService> logger, UserManager<ApplicationUser> userManager, IEventPublisher eventPublisher)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
        _eventPublisher = eventPublisher;
    }

    public async Task<Payment> ProcessPaymentAsync(
        Guid userId,
        Money amount,
        PaymentMethodType paymentMethodType,
        string? paymentMethodId = null,
        string? description = null)
    {
        var payment = new Payment(userId, amount, paymentMethodType, description);
        _context.Payments.Add(payment);

        try
        {
            // Create Stripe PaymentIntent
            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = (long)(amount.Amount * 100), // Convert to cents
                Currency = amount.Currency.ToLower(),
                PaymentMethod = paymentMethodId,
                ConfirmationMethod = "manual",
                Confirm = true,
                Description = description,
                Metadata = new Dictionary<string, string>
                {
                    { "payment_id", payment.Id.ToString() },
                    { "user_id", userId.ToString() }
                }
            });

            payment.SetStripePaymentIntentId(paymentIntent.Id);

            if (paymentIntent.Status == "succeeded")
            {
                payment.Succeed();
                
                // Publish payment processed event
                await _eventPublisher.PublishAsync(new PaymentProcessedEvent
                {
                    PaymentId = payment.Id,
                    UserId = userId,
                    Amount = amount.Amount,
                    Currency = amount.Currency,
                    Status = payment.Status.ToString(),
                    PaymentMethodId = paymentMethodId ?? string.Empty
                });
            }
            else if (paymentIntent.Status == "requires_action")
            {
                payment.Process(paymentIntent.Id);
            }
            else
            {
                payment.Fail($"Payment failed with status: {paymentIntent.Status}");
                
                // Publish payment failed event
                await _eventPublisher.PublishAsync(new PaymentFailedEvent
                {
                    PaymentId = payment.Id,
                    UserId = userId,
                    Amount = amount.Amount,
                    Currency = amount.Currency,
                    FailureReason = $"Payment failed with status: {paymentIntent.Status}"
                });
            }

            await _context.SaveChangesAsync();
            return payment;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe payment processing failed for payment {PaymentId}", payment.Id);
            payment.Fail($"Stripe error: {ex.Message}");
            await _context.SaveChangesAsync();
            return payment;
        }
    }

    public async Task<Payment> ProcessSubscriptionPaymentAsync(
        Guid userId,
        string planId,
        Money amount,
        string? paymentMethodId = null)
    {
        var subscription = new Core.Domain.Entities.Subscription(userId, planId, amount);
        _context.Subscriptions.Add(subscription);

        try
        {
            // Create Stripe Subscription
            var subscriptionService = new Stripe.SubscriptionService();
            StripeSubscription stripeSubscription = await subscriptionService.CreateAsync(new Stripe.SubscriptionCreateOptions
            {
                Customer = await GetOrCreateStripeCustomerAsync(userId),
                Items = new List<Stripe.SubscriptionItemOptions>
                {
                    new Stripe.SubscriptionItemOptions
                    {
                        Price = planId
                    }
                },
                DefaultPaymentMethod = paymentMethodId,
                Metadata = new Dictionary<string, string>
                {
                    { "subscription_id", subscription.Id.ToString() },
                    { "user_id", userId.ToString() }
                }
            });

            subscription.SetStripeSubscriptionId(stripeSubscription.Id);

            // Update subscription status based on Stripe response
            if (stripeSubscription.Status == "active")
            {
                subscription.Activate(DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
            }
            else if (stripeSubscription.Status == "trialing")
            {
                subscription.StartTrial(DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
            }

            await _context.SaveChangesAsync();

            // Create a payment record for the subscription
            var payment = new Payment(userId, amount, PaymentMethodType.Card, $"Subscription for plan {planId}");
            payment.SetStripePaymentIntentId(stripeSubscription.Id); // Use subscription ID as reference
            payment.Succeed();
            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();
            return payment;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe subscription creation failed for user {UserId}", userId);
            subscription.Cancel(DateTime.UtcNow);
            await _context.SaveChangesAsync();
            throw;
        }
    }

    public async Task<bool> RefundPaymentAsync(Guid paymentId, Money? amount = null)
    {
        var payment = await _context.Payments.FindAsync(paymentId);
        if (payment == null || string.IsNullOrEmpty(payment.StripeChargeId))
            return false;

        try
        {
            var refundService = new RefundService();
            var refundOptions = new RefundCreateOptions
            {
                Charge = payment.StripeChargeId,
                Amount = amount != null ? (long)(amount.Amount * 100) : null,
                Metadata = new Dictionary<string, string>
                {
                    { "payment_id", paymentId.ToString() }
                }
            };

            var refund = await refundService.CreateAsync(refundOptions);

            if (amount != null && amount.Amount < payment.Amount.Amount)
            {
                payment.PartialRefund();
            }
            else
            {
                payment.Refund();
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe refund failed for payment {PaymentId}", paymentId);
            return false;
        }
    }

    public async Task<bool> CancelPaymentAsync(Guid paymentId)
    {
        var payment = await _context.Payments.FindAsync(paymentId);
        if (payment == null)
            return false;

        payment.Cancel();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Payment?> GetPaymentAsync(Guid paymentId)
    {
        return await _context.Payments.FindAsync(paymentId);
    }

    public async Task<IEnumerable<Payment>> GetUserPaymentsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        return await _context.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Core.Domain.Entities.Subscription?> GetSubscriptionAsync(Guid subscriptionId)
    {
        return await _context.Subscriptions.FindAsync(subscriptionId);
    }

    public async Task<IEnumerable<Core.Domain.Entities.Subscription>> GetUserSubscriptionsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        return await _context.Subscriptions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> CancelSubscriptionAsync(Guid subscriptionId)
    {
        var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
        if (subscription == null || string.IsNullOrEmpty(subscription.StripeSubscriptionId))
            return false;

        try
        {
            var subscriptionService = new Stripe.SubscriptionService();
            await subscriptionService.CancelAsync(subscription.StripeSubscriptionId);

            subscription.Cancel(DateTime.UtcNow);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe subscription cancellation failed for subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<bool> UpdateSubscriptionAsync(Guid subscriptionId, string planId, Money amount)
    {
        var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
        if (subscription == null || string.IsNullOrEmpty(subscription.StripeSubscriptionId))
            return false;

        try
        {
            var subscriptionService = new Stripe.SubscriptionService();
            var stripeSubscription = await subscriptionService.GetAsync(subscription.StripeSubscriptionId);

            // Update the subscription items
            var subscriptionItemService = new Stripe.SubscriptionItemService();
            var items = await subscriptionItemService.ListAsync(new Stripe.SubscriptionItemListOptions
            {
                Subscription = stripeSubscription.Id
            });

            if (items.Data.Any())
            {
                await subscriptionItemService.UpdateAsync(items.Data.First().Id, new Stripe.SubscriptionItemUpdateOptions
                {
                    Price = planId
                });
            }

            // Update local subscription
            subscription.UpdatePlan(planId, amount);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe subscription update failed for subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    private async Task<string> GetOrCreateStripeCustomerAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        var customerService = new CustomerService();
        var customers = await customerService.ListAsync(new CustomerListOptions
        {
            Email = user.Email!,
            Limit = 1
        });

        if (customers.Data.Any())
        {
            return customers.Data.First().Id;
        }

        var customer = await customerService.CreateAsync(new CustomerCreateOptions
        {
            Email = user.Email!,
            Name = $"{user.FirstName} {user.LastName}",
            Metadata = new Dictionary<string, string>
            {
                { "user_id", userId.ToString() }
            }
        });

        return customer.Id;
    }
}
