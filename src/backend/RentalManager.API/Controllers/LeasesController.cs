// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Queries;

namespace RentalManager.API.Controllers;

/// <summary>
/// Controller for managing leases.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeasesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeasesController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public LeasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a lease by ID.
    /// </summary>
    /// <param name="id">The lease ID.</param>
    /// <returns>The lease details.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<LeaseDto>> GetLeaseById(Guid id)
    {
        var result = await _mediator.Send(new GetLeaseByIdQuery(id));

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets all leases for a property.
    /// </summary>
    /// <param name="propertyId">The property ID.</param>
    /// <returns>List of leases for the property.</returns>
    [HttpGet("property/{propertyId}")]
    public async Task<ActionResult<List<LeaseDto>>> GetLeasesByPropertyId(Guid propertyId)
    {
        var result = await _mediator.Send(new GetLeasesByPropertyIdQuery(propertyId));
        return Ok(result);
    }

    /// <summary>
    /// Gets all leases for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <returns>List of leases for the tenant.</returns>
    [HttpGet("tenant/{tenantId}")]
    public async Task<ActionResult<List<LeaseDto>>> GetLeasesByTenantId(Guid tenantId)
    {
        var result = await _mediator.Send(new GetLeasesByTenantIdQuery(tenantId));
        return Ok(result);
    }

    /// <summary>
    /// Gets the active lease for a property.
    /// </summary>
    /// <param name="propertyId">The property ID.</param>
    /// <returns>The active lease, if any.</returns>
    [HttpGet("property/{propertyId}/active")]
    public async Task<ActionResult<LeaseDto>> GetActiveLeaseByPropertyId(Guid propertyId)
    {
        var result = await _mediator.Send(new GetActiveLeaseByPropertyIdQuery(propertyId));

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Creates a new lease.
    /// </summary>
    /// <param name="leaseData">The lease data.</param>
    /// <returns>The created lease.</returns>
    [HttpPost]
    public async Task<ActionResult<LeaseDto>> CreateLease([FromBody] CreateLeaseDto leaseData)
    {
        var command = new CreateLeaseCommand(leaseData);
        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetLeaseById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Activates a lease.
    /// </summary>
    /// <param name="id">The lease ID.</param>
    /// <returns>The activated lease.</returns>
    [HttpPost("{id}/activate")]
    public async Task<ActionResult<LeaseDto>> ActivateLease(Guid id)
    {
        try
        {
            var command = new ActivateLeaseCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Terminates a lease.
    /// </summary>
    /// <param name="id">The lease ID.</param>
    /// <param name="request">The termination request with reason.</param>
    /// <returns>The terminated lease.</returns>
    [HttpPost("{id}/terminate")]
    public async Task<ActionResult<LeaseDto>> TerminateLease(Guid id, [FromBody] TerminateLeaseRequest request)
    {
        try
        {
            var command = new TerminateLeaseCommand(id, request.Reason);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Renews a lease.
    /// </summary>
    /// <param name="id">The lease ID.</param>
    /// <param name="renewalData">The renewal data.</param>
    /// <returns>The renewed lease.</returns>
    [HttpPost("{id}/renew")]
    public async Task<ActionResult<LeaseDto>> RenewLease(Guid id, [FromBody] RenewLeaseDto renewalData)
    {
        try
        {
            var command = new RenewLeaseCommand(id, renewalData);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetLeaseById), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates the rent amount for a lease.
    /// </summary>
    /// <param name="id">The lease ID.</param>
    /// <param name="rentData">The new rent data.</param>
    /// <returns>The updated lease.</returns>
    [HttpPut("{id}/rent")]
    public async Task<ActionResult<LeaseDto>> UpdateLeaseRent(Guid id, [FromBody] UpdateLeaseRentDto rentData)
    {
        try
        {
            var command = new UpdateLeaseRentCommand(id, rentData);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

/// <summary>
/// Request model for terminating a lease.
/// </summary>
public class TerminateLeaseRequest
{
    /// <summary>
    /// Gets or sets the termination reason.
    /// </summary>
    public string Reason { get; set; } = default!;
}
