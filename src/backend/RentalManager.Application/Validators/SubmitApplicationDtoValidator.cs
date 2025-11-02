// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentValidation;
using RentalManager.Application.DTOs;

namespace RentalManager.Application.Validators;

/// <summary>
/// Validator for SubmitApplicationDto.
/// </summary>
public class SubmitApplicationDtoValidator : AbstractValidator<SubmitApplicationDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubmitApplicationDtoValidator"/> class.
    /// </summary>
    public SubmitApplicationDtoValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("Property ID is required");

        RuleFor(x => x.ApplicationData)
            .NotNull().WithMessage("Application data is required")
            .SetValidator(new ApplicationDataDtoValidator());
    }
}

/// <summary>
/// Validator for ApplicationDataDto.
/// </summary>
public class ApplicationDataDtoValidator : AbstractValidator<ApplicationDataDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDataDtoValidator"/> class.
    /// </summary>
    public ApplicationDataDtoValidator()
    {
        // Personal Information
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address format")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .Matches(@"^[\d\s\-\(\)\+]+$").WithMessage("Phone number contains invalid characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.Now.AddYears(-18)).WithMessage("Applicant must be at least 18 years old")
            .GreaterThan(DateTime.Now.AddYears(-120)).WithMessage("Invalid date of birth");

        // Employment Information
        RuleFor(x => x.EmployerName)
            .NotEmpty().WithMessage("Employer name is required")
            .MaximumLength(200).WithMessage("Employer name must not exceed 200 characters");

        RuleFor(x => x.JobTitle)
            .NotEmpty().WithMessage("Job title is required")
            .MaximumLength(100).WithMessage("Job title must not exceed 100 characters");

        RuleFor(x => x.AnnualIncome)
            .GreaterThan(0).WithMessage("Annual income must be greater than 0")
            .LessThanOrEqualTo(100000000).WithMessage("Annual income value is too high");

        RuleFor(x => x.YearsEmployed)
            .GreaterThanOrEqualTo(0).WithMessage("Years employed must be 0 or greater")
            .LessThanOrEqualTo(80).WithMessage("Years employed must not exceed 80");

        // Previous Addresses
        RuleFor(x => x.PreviousAddresses)
            .NotNull().WithMessage("Previous addresses list is required")
            .Must(list => list.Count <= 20).WithMessage("Cannot have more than 20 previous addresses");

        RuleForEach(x => x.PreviousAddresses)
            .SetValidator(new PreviousAddressDtoValidator());

        // References
        RuleFor(x => x.References)
            .NotNull().WithMessage("References list is required")
            .Must(list => list.Count <= 10).WithMessage("Cannot have more than 10 references");

        RuleForEach(x => x.References)
            .SetValidator(new ReferenceDtoValidator());

        // Additional Notes
        RuleFor(x => x.AdditionalNotes)
            .MaximumLength(2000).WithMessage("Additional notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.AdditionalNotes));

        // Terms Acceptance
        RuleFor(x => x.TermsAccepted)
            .Must(x => x == true).WithMessage("Terms and conditions must be accepted");
    }
}

/// <summary>
/// Validator for PreviousAddressDto.
/// </summary>
public class PreviousAddressDtoValidator : AbstractValidator<PreviousAddressDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PreviousAddressDtoValidator"/> class.
    /// </summary>
    public PreviousAddressDtoValidator()
    {
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters");

        RuleFor(x => x.MoveInDate)
            .NotEmpty().WithMessage("Move-in date is required")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Move-in date cannot be in the future");

        RuleFor(x => x.MoveOutDate)
            .NotEmpty().WithMessage("Move-out date is required")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Move-out date cannot be in the future")
            .GreaterThan(x => x.MoveInDate).WithMessage("Move-out date must be after move-in date");

        RuleFor(x => x.LandlordName)
            .NotEmpty().WithMessage("Landlord name is required")
            .MaximumLength(200).WithMessage("Landlord name must not exceed 200 characters");

        RuleFor(x => x.LandlordPhone)
            .NotEmpty().WithMessage("Landlord phone is required")
            .MaximumLength(20).WithMessage("Landlord phone must not exceed 20 characters")
            .Matches(@"^[\d\s\-\(\)\+]+$").WithMessage("Landlord phone contains invalid characters");

        RuleFor(x => x.MonthlyRent)
            .GreaterThanOrEqualTo(0).WithMessage("Monthly rent must be 0 or greater")
            .LessThanOrEqualTo(1000000).WithMessage("Monthly rent value is too high");

        RuleFor(x => x.ReasonForLeaving)
            .NotEmpty().WithMessage("Reason for leaving is required")
            .MaximumLength(500).WithMessage("Reason for leaving must not exceed 500 characters");
    }
}

/// <summary>
/// Validator for ReferenceDto.
/// </summary>
public class ReferenceDtoValidator : AbstractValidator<ReferenceDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceDtoValidator"/> class.
    /// </summary>
    public ReferenceDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Reference name is required")
            .MaximumLength(200).WithMessage("Reference name must not exceed 200 characters");

        RuleFor(x => x.Relationship)
            .NotEmpty().WithMessage("Relationship is required")
            .MaximumLength(100).WithMessage("Relationship must not exceed 100 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Reference phone is required")
            .MaximumLength(20).WithMessage("Reference phone must not exceed 20 characters")
            .Matches(@"^[\d\s\-\(\)\+]+$").WithMessage("Reference phone contains invalid characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Reference email is required")
            .EmailAddress().WithMessage("Invalid email address format")
            .MaximumLength(200).WithMessage("Reference email must not exceed 200 characters");
    }
}