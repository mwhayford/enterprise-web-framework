// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManager.Application.Commands;

namespace RentalManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BackgroundJobsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BackgroundJobsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("send-welcome-email")]
    public async Task<ActionResult> SendWelcomeEmail([FromBody] SendWelcomeEmailCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Welcome email queued for sending" });
    }

    [HttpPost("send-payment-confirmation-email")]
    public async Task<ActionResult> SendPaymentConfirmationEmail([FromBody] SendPaymentConfirmationEmailCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Payment confirmation email queued for sending" });
    }

    [HttpPost("process-user-data")]
    public async Task<ActionResult> ProcessUserData([FromBody] ProcessUserDataCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "User data processing queued" });
    }
}
