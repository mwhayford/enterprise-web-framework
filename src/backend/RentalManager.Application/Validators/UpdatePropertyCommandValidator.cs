// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentValidation;
using RentalManager.Application.Commands;

namespace RentalManager.Application.Validators;

/// <summary>
/// Validator for UpdatePropertyCommand.
/// </summary>
public class UpdatePropertyCommandValidator : AbstractValidator<UpdatePropertyCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePropertyCommandValidator"/> class.
    /// </summary>
    public UpdatePropertyCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("Property ID is required");

        RuleFor(x => x.PropertyData)
            .NotNull().WithMessage("Property data is required")
            .SetValidator(new CreatePropertyDtoValidator());
    }
}
