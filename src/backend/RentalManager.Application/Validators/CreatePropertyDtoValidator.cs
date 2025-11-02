// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentValidation;
using RentalManager.Application.DTOs;

namespace RentalManager.Application.Validators;

/// <summary>
/// Validator for CreatePropertyDto used in commands.
/// </summary>
public class CreatePropertyDtoValidator : AbstractValidator<CreatePropertyDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePropertyDtoValidator"/> class.
    /// </summary>
    public CreatePropertyDtoValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street address is required")
            .MaximumLength(200).WithMessage("Street address must not exceed 200 characters");

        RuleFor(x => x.Unit)
            .MaximumLength(50).WithMessage("Unit must not exceed 50 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required")
            .MaximumLength(100).WithMessage("State must not exceed 100 characters");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required")
            .MaximumLength(20).WithMessage("Zip code must not exceed 20 characters");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters");

        RuleFor(x => x.PropertyType)
            .IsInEnum().WithMessage("Invalid property type");

        RuleFor(x => x.Bedrooms)
            .GreaterThanOrEqualTo(0).WithMessage("Bedrooms must be 0 or greater")
            .LessThanOrEqualTo(50).WithMessage("Bedrooms must not exceed 50");

        RuleFor(x => x.Bathrooms)
            .GreaterThanOrEqualTo(0).WithMessage("Bathrooms must be 0 or greater")
            .LessThanOrEqualTo(50).WithMessage("Bathrooms must not exceed 50");

        RuleFor(x => x.SquareFeet)
            .GreaterThan(0).WithMessage("Square feet must be greater than 0")
            .LessThanOrEqualTo(1000000).WithMessage("Square feet must not exceed 1,000,000");

        RuleFor(x => x.MonthlyRent)
            .GreaterThan(0).WithMessage("Monthly rent must be greater than 0")
            .LessThanOrEqualTo(1000000).WithMessage("Monthly rent must not exceed 1,000,000");

        RuleFor(x => x.RentCurrency)
            .MaximumLength(3).WithMessage("Currency code must not exceed 3 characters")
            .Must(BeValidCurrencyCode).WithMessage("Invalid currency code");

        RuleFor(x => x.SecurityDeposit)
            .GreaterThanOrEqualTo(0).WithMessage("Security deposit must be 0 or greater")
            .LessThanOrEqualTo(1000000).WithMessage("Security deposit must not exceed 1,000,000");

        RuleFor(x => x.SecurityDepositCurrency)
            .MaximumLength(3).WithMessage("Currency code must not exceed 3 characters")
            .Must(BeValidCurrencyCode).WithMessage("Invalid currency code");

        RuleFor(x => x.AvailableDate)
            .NotEmpty().WithMessage("Available date is required");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters");

        RuleFor(x => x.ApplicationFee)
            .GreaterThanOrEqualTo(0).WithMessage("Application fee must be 0 or greater")
            .LessThanOrEqualTo(10000).WithMessage("Application fee must not exceed 10,000")
            .When(x => x.ApplicationFee.HasValue);

        RuleFor(x => x.ApplicationFeeCurrency)
            .MaximumLength(3).WithMessage("Currency code must not exceed 3 characters")
            .Must(BeValidCurrencyCode).WithMessage("Invalid currency code")
            .When(x => !string.IsNullOrWhiteSpace(x.ApplicationFeeCurrency));

        RuleFor(x => x.Amenities)
            .Must(list => list == null || list.Count <= 100)
            .WithMessage("Amenities list must not exceed 100 items");

        RuleForEach(x => x.Amenities)
            .MaximumLength(200).WithMessage("Each amenity must not exceed 200 characters")
            .When(x => x.Amenities != null);

        RuleFor(x => x.Images)
            .Must(list => list == null || list.Count <= 50)
            .WithMessage("Images list must not exceed 50 items");

        RuleForEach(x => x.Images)
            .MaximumLength(2000).WithMessage("Each image URL must not exceed 2000 characters")
            .When(x => x.Images != null);
    }

    private static bool BeValidCurrencyCode(string? currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            return true;
        }

        var validCurrencies = new[]
        {
            "USD", "EUR", "GBP", "CAD", "AUD", "JPY", "CNY", "INR", "MXN", "BRL",
        };

        return validCurrencies.Contains(currencyCode.ToUpperInvariant());
    }
}
