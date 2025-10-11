// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Core.Application.Commands;
using MediatR;

namespace Core.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentMethodsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentMethodsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePaymentMethod([FromBody] CreatePaymentMethodCommand command)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        command = command with { UserId = userGuid };
        var paymentMethod = await _mediator.Send(command);
        return Ok(paymentMethod);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaymentMethods()
    {
        // This would need a GetPaymentMethodsQuery implementation
        return Ok(new { Message = "Get payment methods endpoint - to be implemented" });
    }

    [HttpDelete("{paymentMethodId}")]
    public async Task<IActionResult> DeletePaymentMethod(Guid paymentMethodId)
    {
        // This would need a DeletePaymentMethodCommand implementation
        return Ok(new { Message = "Delete payment method endpoint - to be implemented" });
    }

    [HttpPut("{paymentMethodId}/set-default")]
    public async Task<IActionResult> SetDefaultPaymentMethod(Guid paymentMethodId)
    {
        // This would need a SetDefaultPaymentMethodCommand implementation
        return Ok(new { Message = "Set default payment method endpoint - to be implemented" });
    }
}
