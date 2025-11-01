// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Queries;

namespace RentalManager.Application.Handlers;

public class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, PropertyDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPropertyByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyDto?> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken);

        if (property == null)
        {
            return null;
        }

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
