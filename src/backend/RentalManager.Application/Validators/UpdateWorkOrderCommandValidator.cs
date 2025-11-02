// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using FluentValidation;
using RentalManager.Application.Commands;

namespace RentalManager.Application.Validators;

public class UpdateWorkOrderCommandValidator : AbstractValidator<UpdateWorkOrderCommand>
{
    public UpdateWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("Work order ID is required");

        RuleFor(x => x.UpdateData.Title)
            .MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.UpdateData.Title))
            .WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.UpdateData.Description)
            .MaximumLength(5000).When(x => !string.IsNullOrWhiteSpace(x.UpdateData.Description))
            .WithMessage("Description cannot exceed 5000 characters");

        RuleFor(x => x.UpdateData.EstimatedCost)
            .GreaterThanOrEqualTo(0).When(x => x.UpdateData.EstimatedCost.HasValue)
            .WithMessage("Estimated cost cannot be negative");

        RuleFor(x => x.UpdateData.Notes)
            .MaximumLength(5000).When(x => !string.IsNullOrWhiteSpace(x.UpdateData.Notes))
            .WithMessage("Notes cannot exceed 5000 characters");
    }
}