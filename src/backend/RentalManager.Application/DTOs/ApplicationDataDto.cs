// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace RentalManager.Application.DTOs;

public class ApplicationDataDto
{
    // Personal Information
    public string FirstName { get; set; } = default!;

    public string LastName { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string Phone { get; set; } = default!;

    public DateTime DateOfBirth { get; set; }

    // Employment Information
    public string EmployerName { get; set; } = default!;

    public string JobTitle { get; set; } = default!;

    public decimal AnnualIncome { get; set; }

    public int YearsEmployed { get; set; }

    // Previous Addresses (Rental History)
    public List<PreviousAddressDto> PreviousAddresses { get; set; } = new();

    // References
    public List<ReferenceDto> References { get; set; } = new();

    // Additional Notes
    public string? AdditionalNotes { get; set; }

    // Terms Acceptance
    public bool TermsAccepted { get; set; }
}

public class PreviousAddressDto
{
    public string Address { get; set; } = default!;

    public DateTime MoveInDate { get; set; }

    public DateTime MoveOutDate { get; set; }

    public string LandlordName { get; set; } = default!;

    public string LandlordPhone { get; set; } = default!;

    public decimal MonthlyRent { get; set; }

    public string ReasonForLeaving { get; set; } = default!;
}

public class ReferenceDto
{
    public string Name { get; set; } = default!;

    public string Relationship { get; set; } = default!;

    public string Phone { get; set; } = default!;

    public string Email { get; set; } = default!;
}
