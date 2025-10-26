// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentValidation;
using RentalManager.Application.Commands;

namespace RentalManager.Application.Validators;

/// <summary>
/// Validator for SubmitPropertyApplicationCommand.
/// </summary>
public class SubmitPropertyApplicationCommandValidator : AbstractValidator<SubmitPropertyApplicationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubmitPropertyApplicationCommandValidator"/> class.
    /// </summary>
    public SubmitPropertyApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationData)
            .NotNull().WithMessage("Application data is required")
            .SetValidator(new SubmitApplicationDtoValidator());
    }
}

