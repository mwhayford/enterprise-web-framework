// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.Commands;
using RentalManager.Application.Interfaces;

namespace RentalManager.Application.Handlers;

public class ConfirmApplicationFeeCommandHandler : IRequestHandler<ConfirmApplicationFeeCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public ConfirmApplicationFeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ConfirmApplicationFeeCommand request, CancellationToken cancellationToken)
    {
        var application = await _context.PropertyApplications
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application == null)
        {
            throw new KeyNotFoundException("Application not found");
        }

        // Attach payment to application
        application.AttachPayment(Guid.Parse(request.PaymentIntentId));

        await _context.SaveChangesAsync(cancellationToken);

        // Schedule background job to send payment confirmation email
        BackgroundJob.Enqueue<IApplicationNotificationJobs>(x =>
            x.SendApplicationFeePaymentConfirmationAsync(application.Id));

        return Unit.Value;
    }
}