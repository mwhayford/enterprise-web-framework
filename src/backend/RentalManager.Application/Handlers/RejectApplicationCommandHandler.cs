// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;

namespace RentalManager.Application.Handlers;

public class RejectApplicationCommandHandler : IRequestHandler<RejectApplicationCommand, PropertyApplicationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RejectApplicationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PropertyApplicationDto> Handle(RejectApplicationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User must be authenticated");

        var application = await _context.PropertyApplications
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken)
            ?? throw new InvalidOperationException("Application not found");

        application.Reject(userId, request.DecisionNotes);

        await _context.SaveChangesAsync(cancellationToken);

        // Schedule background job to send rejection email
        BackgroundJob.Enqueue<IApplicationNotificationJobs>(x =>
            x.SendApplicationStatusUpdateEmailAsync(application.Id, "Rejected"));

        return new PropertyApplicationDto
        {
            Id = application.Id,
            PropertyId = application.PropertyId,
            ApplicantId = application.ApplicantId,
            Status = application.Status,
            ApplicationData = application.ApplicationData,
            ApplicationFee = application.ApplicationFee.Amount,
            ApplicationFeeCurrency = application.ApplicationFee.Currency,
            ApplicationFeePaymentId = application.ApplicationFeePaymentId,
            SubmittedAt = application.SubmittedAt,
            ReviewedAt = application.ReviewedAt,
            ReviewedBy = application.ReviewedBy,
            DecisionNotes = application.DecisionNotes,
            CreatedAt = application.CreatedAt,
            UpdatedAt = application.UpdatedAt
        };
    }
}

