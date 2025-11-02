// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using FluentValidation;
using RentalManager.Application.Commands;

namespace RentalManager.Application.Validators;

public class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderData.PropertyId)
            .NotEmpty().WithMessage("Property ID is required");

        RuleFor(x => x.WorkOrderData.LeaseId)
            .NotEmpty().WithMessage("Lease ID is required");

        RuleFor(x => x.WorkOrderData.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.WorkOrderData.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters");

        RuleFor(x => x.WorkOrderData.Category)
            .IsInEnum().WithMessage("Invalid category");

        RuleFor(x => x.WorkOrderData.Priority)
            .IsInEnum().WithMessage("Invalid priority");
    }
}