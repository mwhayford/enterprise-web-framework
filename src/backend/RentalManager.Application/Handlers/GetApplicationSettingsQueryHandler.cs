// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Queries;
using RentalManager.Domain.Entities;

namespace RentalManager.Application.Handlers;

public class GetApplicationSettingsQueryHandler : IRequestHandler<GetApplicationSettingsQuery, ApplicationSettingsDto>
{
    private readonly IApplicationDbContext _context;

    public GetApplicationSettingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationSettingsDto> Handle(GetApplicationSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _context.ApplicationSettings.FirstOrDefaultAsync(cancellationToken);

        // If no settings exist, create default settings
        if (settings == null)
        {
            settings = ApplicationSettings.CreateDefault();
            _context.ApplicationSettings.Add(settings);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new ApplicationSettingsDto
        {
            Id = settings.Id,
            DefaultApplicationFee = settings.DefaultApplicationFee.Amount,
            DefaultApplicationFeeCurrency = settings.DefaultApplicationFee.Currency,
            ApplicationFeeEnabled = settings.ApplicationFeeEnabled,
            RequirePaymentUpfront = settings.RequirePaymentUpfront,
            MaxApplicationsPerUser = settings.MaxApplicationsPerUser,
            ApplicationFormFields = settings.ApplicationFormFields,
            UpdatedBy = settings.UpdatedBy,
            UpdatedAt = settings.UpdatedAt
        };
    }
}