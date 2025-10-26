// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Queries;

namespace RentalManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ApplicationsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<ActionResult<PropertyApplicationDto>> SubmitApplication([FromBody] SubmitApplicationDto applicationData)
    {
        var command = new SubmitPropertyApplicationCommand(applicationData);
        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetApplicationById), new { id = result.Id }, result);
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<PropertyApplicationDto>>> GetMyApplications()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var query = new GetUserApplicationsQuery(userId.Value);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PropertyApplicationDto>> GetApplicationById(Guid id)
    {
        var query = new GetApplicationByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin,PropertyOwner")]
    public async Task<ActionResult<PropertyApplicationDto>> ApproveApplication(Guid id, [FromBody] DecisionRequest request)
    {
        var command = new ApproveApplicationCommand(id, request.DecisionNotes);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin,PropertyOwner")]
    public async Task<ActionResult<PropertyApplicationDto>> RejectApplication(Guid id, [FromBody] DecisionRequest request)
    {
        var command = new RejectApplicationCommand(id, request.DecisionNotes);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPost("{id}/pay-fee")]
    public async Task<ActionResult<PaymentIntentResponse>> PayApplicationFee(Guid id)
    {
        var command = new ProcessApplicationFeeCommand(id);
        var clientSecret = await _mediator.Send(command);

        return Ok(new PaymentIntentResponse { ClientSecret = clientSecret });
    }

    [HttpPost("{id}/confirm-payment")]
    public async Task<ActionResult> ConfirmPayment(Guid id, [FromBody] ConfirmPaymentRequest request)
    {
        var command = new ConfirmApplicationFeeCommand(id, request.PaymentIntentId);
        await _mediator.Send(command);

        return Ok();
    }
}

public class DecisionRequest
{
    public string? DecisionNotes { get; set; }
}

public class PaymentIntentResponse
{
    public string ClientSecret { get; set; } = string.Empty;
}

public class ConfirmPaymentRequest
{
    public string PaymentIntentId { get; set; } = string.Empty;
}

