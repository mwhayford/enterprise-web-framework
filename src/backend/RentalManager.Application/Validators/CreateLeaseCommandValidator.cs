// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using FluentValidation;
using RentalManager.Application.Commands;

namespace RentalManager.Application.Validators;

public class CreateLeaseCommandValidator : AbstractValidator<CreateLeaseCommand>
{
    public CreateLeaseCommandValidator()
    {
        RuleFor(x => x.LeaseData.PropertyId)
            .NotEmpty().WithMessage("Property ID is required");

        RuleFor(x => x.LeaseData.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");

        RuleFor(x => x.LeaseData.LandlordId)
            .NotEmpty().WithMessage("Landlord ID is required");

        RuleFor(x => x.LeaseData.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThan(x => x.LeaseData.EndDate).WithMessage("Start date must be before end date");

        RuleFor(x => x.LeaseData.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.LeaseData.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.LeaseData.MonthlyRent)
            .GreaterThan(0).WithMessage("Monthly rent must be greater than zero");

        RuleFor(x => x.LeaseData.RentCurrency)
            .NotEmpty().WithMessage("Rent currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code");

        RuleFor(x => x.LeaseData.SecurityDeposit)
            .GreaterThanOrEqualTo(0).WithMessage("Security deposit cannot be negative");

        RuleFor(x => x.LeaseData.SecurityDepositCurrency)
            .NotEmpty().WithMessage("Security deposit currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code");

        RuleFor(x => x.LeaseData.PaymentDayOfMonth)
            .InclusiveBetween(1, 28).WithMessage("Payment day of month must be between 1 and 28");

        RuleFor(x => x.LeaseData.SpecialTerms)
            .MaximumLength(5000).WithMessage("Special terms cannot exceed 5000 characters");
    }
}

