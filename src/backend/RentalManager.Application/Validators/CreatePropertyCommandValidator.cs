// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentValidation;
using RentalManager.Application.Commands;

namespace RentalManager.Application.Validators;

/// <summary>
/// Validator for CreatePropertyCommand.
/// </summary>
public class CreatePropertyCommandValidator : AbstractValidator<CreatePropertyCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePropertyCommandValidator"/> class.
    /// </summary>
    public CreatePropertyCommandValidator()
    {
        RuleFor(x => x.PropertyData)
            .NotNull().WithMessage("Property data is required")
            .SetValidator(new CreatePropertyDtoValidator());
    }
}

