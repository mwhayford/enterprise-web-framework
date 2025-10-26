// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using FluentValidation;
using RentalManager.Application.Commands;

namespace RentalManager.Application.Validators;

public class UpdateLeaseRentCommandValidator : AbstractValidator<UpdateLeaseRentCommand>
{
    public UpdateLeaseRentCommandValidator()
    {
        RuleFor(x => x.LeaseId)
            .NotEmpty().WithMessage("Lease ID is required");

        RuleFor(x => x.RentData.MonthlyRent)
            .GreaterThan(0).WithMessage("Monthly rent must be greater than zero");

        RuleFor(x => x.RentData.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code");
    }
}

