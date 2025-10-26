// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentValidation;
using RentalManager.Application.Commands;

namespace RentalManager.Application.Validators;

/// <summary>
/// Validator for ApproveApplicationCommand.
/// </summary>
public class ApproveApplicationCommandValidator : AbstractValidator<ApproveApplicationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApproveApplicationCommandValidator"/> class.
    /// </summary>
    public ApproveApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required");

        RuleFor(x => x.DecisionNotes)
            .MaximumLength(2000).WithMessage("Decision notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.DecisionNotes));
    }
}

