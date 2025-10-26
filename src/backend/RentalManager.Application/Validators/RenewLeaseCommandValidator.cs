// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using FluentValidation;
using RentalManager.Application.Commands;

namespace RentalManager.Application.Validators;

public class RenewLeaseCommandValidator : AbstractValidator<RenewLeaseCommand>
{
    public RenewLeaseCommandValidator()
    {
        RuleFor(x => x.LeaseId)
            .NotEmpty().WithMessage("Lease ID is required");

        RuleFor(x => x.RenewalData.NewEndDate)
            .NotEmpty().WithMessage("New end date is required")
            .GreaterThan(DateTime.UtcNow).WithMessage("New end date must be in the future");

        When(x => x.RenewalData.NewMonthlyRent.HasValue, () =>
        {
            RuleFor(x => x.RenewalData.NewMonthlyRent!.Value)
                .GreaterThan(0).WithMessage("New monthly rent must be greater than zero");

            RuleFor(x => x.RenewalData.Currency)
                .NotEmpty().WithMessage("Currency is required when updating rent")
                .Length(3).WithMessage("Currency must be a 3-letter ISO code");
        });
    }
}

