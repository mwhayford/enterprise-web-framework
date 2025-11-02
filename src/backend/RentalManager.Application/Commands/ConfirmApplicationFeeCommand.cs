// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using MediatR;

namespace RentalManager.Application.Commands;

public record ConfirmApplicationFeeCommand(Guid ApplicationId, string PaymentIntentId) : IRequest<Unit>;