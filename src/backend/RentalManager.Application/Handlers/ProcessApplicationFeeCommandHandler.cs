// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RentalManager.Application.Commands;
using RentalManager.Application.Interfaces;
using Stripe;

namespace RentalManager.Application.Handlers;

public class ProcessApplicationFeeCommandHandler : IRequestHandler<ProcessApplicationFeeCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public ProcessApplicationFeeCommandHandler(
        IApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string> Handle(ProcessApplicationFeeCommand request, CancellationToken cancellationToken)
    {
        var application = await _context.PropertyApplications
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application == null)
        {
            throw new KeyNotFoundException("Application not found");
        }

        if (application.ApplicationFeePaymentId != null)
        {
            throw new InvalidOperationException("Application fee has already been paid");
        }

        // Initialize Stripe
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

        // Create payment intent
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(application.ApplicationFee.Amount * 100), // Convert to cents
            Currency = application.ApplicationFee.Currency.ToLowerInvariant(),
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
            Metadata = new Dictionary<string, string>
            {
                { "ApplicationId", application.Id.ToString() },
                { "PropertyId", application.PropertyId.ToString() },
                { "ApplicantId", application.ApplicantId.ToString() },
            },
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

        // Return client secret for frontend
        return paymentIntent.ClientSecret;
    }
}
