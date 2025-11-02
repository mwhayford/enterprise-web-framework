// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using RentalManager.Application.DTOs;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.Queries;

public record GetApplicationsByOwnerIdQuery(Guid OwnerId, ApplicationStatus? Status = null) : IRequest<List<PropertyApplicationDto>>;