// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.Handlers;

public class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand, PropertyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePropertyCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PropertyDto> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User must be authenticated");

        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken)
            ?? throw new InvalidOperationException("Property not found");

        // Authorization: Only owner can update
        if (property.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to update this property");
        }

        var monthlyRent = Money.Create(request.PropertyData.MonthlyRent, request.PropertyData.RentCurrency);
        var securityDeposit = Money.Create(request.PropertyData.SecurityDeposit, request.PropertyData.SecurityDepositCurrency);

        property.UpdateDetails(
            request.PropertyData.Bedrooms,
            request.PropertyData.Bathrooms,
            request.PropertyData.SquareFeet,
            monthlyRent,
            securityDeposit,
            request.PropertyData.AvailableDate,
            request.PropertyData.Description);

        if (request.PropertyData.ApplicationFee.HasValue && !string.IsNullOrEmpty(request.PropertyData.ApplicationFeeCurrency))
        {
            var appFee = Money.Create(request.PropertyData.ApplicationFee.Value, request.PropertyData.ApplicationFeeCurrency);
            property.UpdateApplicationFee(appFee);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new PropertyDto
        {
            Id = property.Id,
            OwnerId = property.OwnerId,
            Street = property.Address.Street,
            Unit = property.Address.Unit,
            City = property.Address.City,
            State = property.Address.State,
            ZipCode = property.Address.ZipCode,
            Country = property.Address.Country,
            PropertyType = property.PropertyType,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            SquareFeet = property.SquareFeet,
            MonthlyRent = property.MonthlyRent.Amount,
            RentCurrency = property.MonthlyRent.Currency,
            SecurityDeposit = property.SecurityDeposit.Amount,
            SecurityDepositCurrency = property.SecurityDeposit.Currency,
            AvailableDate = property.AvailableDate,
            Status = property.Status,
            Description = property.Description,
            ApplicationFee = property.ApplicationFee?.Amount,
            ApplicationFeeCurrency = property.ApplicationFee?.Currency,
            Amenities = property.Amenities.ToList(),
            Images = property.Images.ToList(),
            CreatedAt = property.CreatedAt,
            UpdatedAt = property.UpdatedAt
        };
    }
}
