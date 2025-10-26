// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using FluentValidation;
using RentalManager.Application.Commands;

namespace RentalManager.Application.Validators;

public class TerminateLeaseCommandValidator : AbstractValidator<TerminateLeaseCommand>
{
    public TerminateLeaseCommandValidator()
    {
        RuleFor(x => x.LeaseId)
            .NotEmpty().WithMessage("Lease ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Termination reason is required")
            .MinimumLength(10).WithMessage("Termination reason must be at least 10 characters")
            .MaximumLength(1000).WithMessage("Termination reason cannot exceed 1000 characters");
    }
}

