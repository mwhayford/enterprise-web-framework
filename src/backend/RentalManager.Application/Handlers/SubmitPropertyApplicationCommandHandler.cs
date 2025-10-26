// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Text.Json;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Domain.Entities;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.Handlers;

public class SubmitPropertyApplicationCommandHandler : IRequestHandler<SubmitPropertyApplicationCommand, PropertyApplicationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SubmitPropertyApplicationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PropertyApplicationDto> Handle(SubmitPropertyApplicationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User must be authenticated");

        // Get property to determine application fee
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.ApplicationData.PropertyId, cancellationToken)
            ?? throw new InvalidOperationException("Property not found");

        // Get default application settings
        var settings = await _context.ApplicationSettings
            .FirstOrDefaultAsync(cancellationToken);

        // Determine application fee (property-specific or default)
        Money applicationFee;
        if (property.ApplicationFee != null)
        {
            applicationFee = property.ApplicationFee;
        }
        else if (settings != null)
        {
            applicationFee = settings.DefaultApplicationFee;
        }
        else
        {
            // Fallback to default $35
            applicationFee = Money.Create(35.00m, "USD");
        }

        // Serialize application data
        var applicationDataJson = JsonSerializer.Serialize(request.ApplicationData.ApplicationData);

        var application = new PropertyApplication(
            request.ApplicationData.PropertyId,
            userId,
            applicationDataJson,
            applicationFee);

        // Submit the application
        application.Submit();

        _context.PropertyApplications.Add(application);
        await _context.SaveChangesAsync(cancellationToken);

        // Schedule background job to send notification emails
        BackgroundJob.Enqueue<IApplicationNotificationJobs>(x =>
            x.SendApplicationSubmittedEmailAsync(application.Id));
        BackgroundJob.Enqueue<IApplicationNotificationJobs>(x =>
            x.SendPropertyOwnerNewApplicationNotificationAsync(application.Id));

        return MapToDto(application);
    }

    private static PropertyApplicationDto MapToDto(PropertyApplication application)
    {
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

