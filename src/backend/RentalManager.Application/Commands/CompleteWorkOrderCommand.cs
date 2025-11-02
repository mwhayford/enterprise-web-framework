// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using RentalManager.Application.DTOs;

namespace RentalManager.Application.Commands;

public record CompleteWorkOrderCommand(Guid WorkOrderId, decimal? ActualCost = null, string? Notes = null) : IRequest<WorkOrderDto>;

