// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using FluentValidation;
using RentalManager.Application.Commands;

namespace RentalManager.Application.Validators;

public class AssignWorkOrderCommandValidator : AbstractValidator<AssignWorkOrderCommand>
{
    public AssignWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("Work order ID is required");

        RuleFor(x => x.ContractorId)
            .NotEmpty().WithMessage("Contractor ID is required");
    }
}


