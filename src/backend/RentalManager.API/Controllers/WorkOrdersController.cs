// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Queries;
using RentalManager.Domain.Constants;

namespace RentalManager.API.Controllers;

/// <summary>
/// Controller for managing work orders.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkOrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkOrdersController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    /// <param name="currentUserService">The current user service.</param>
    public WorkOrdersController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Creates a new work order.
    /// </summary>
    /// <param name="workOrderData">The work order data.</param>
    /// <returns>The created work order.</returns>
    [HttpPost]
    [Authorize(Roles = Roles.Resident)]
    public async Task<ActionResult<WorkOrderDto>> CreateWorkOrder([FromBody] CreateWorkOrderDto workOrderData)
    {
        try
        {
            var command = new CreateWorkOrderCommand(workOrderData);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetWorkOrderById), new { id = result.Id }, result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a work order by ID.
    /// </summary>
    /// <param name="id">The work order ID.</param>
    /// <returns>The work order details.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkOrderDto>> GetWorkOrderById(Guid id)
    {
        var result = await _mediator.Send(new GetWorkOrderByIdQuery(id));

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets all work orders for the current resident.
    /// </summary>
    /// <returns>List of work orders.</returns>
    [HttpGet("my")]
    [Authorize(Roles = Roles.Resident)]
    public async Task<ActionResult<List<WorkOrderDto>>> GetMyWorkOrders()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new GetWorkOrdersByResidentIdQuery(userId.Value));
        return Ok(result);
    }

    /// <summary>
    /// Gets all work orders for a property.
    /// </summary>
    /// <param name="propertyId">The property ID.</param>
    /// <returns>List of work orders for the property.</returns>
    [HttpGet("property/{propertyId}")]
    public async Task<ActionResult<List<WorkOrderDto>>> GetWorkOrdersByPropertyId(Guid propertyId)
    {
        var result = await _mediator.Send(new GetWorkOrdersByPropertyIdQuery(propertyId));
        return Ok(result);
    }

    /// <summary>
    /// Gets all work orders for a resident.
    /// </summary>
    /// <param name="residentId">The resident ID.</param>
    /// <returns>List of work orders for the resident.</returns>
    [HttpGet("resident/{residentId}")]
    [Authorize(Roles = $"{Roles.Resident},{Roles.Owner},{Roles.Admin}")]
    public async Task<ActionResult<List<WorkOrderDto>>> GetWorkOrdersByResidentId(Guid residentId)
    {
        var result = await _mediator.Send(new GetWorkOrdersByResidentIdQuery(residentId));
        return Ok(result);
    }

    /// <summary>
    /// Gets all work orders assigned to a contractor.
    /// </summary>
    /// <param name="contractorId">The contractor ID.</param>
    /// <returns>List of work orders assigned to the contractor.</returns>
    [HttpGet("contractor/{contractorId}")]
    [Authorize(Roles = $"{Roles.Contractor},{Roles.Owner},{Roles.Admin}")]
    public async Task<ActionResult<List<WorkOrderDto>>> GetWorkOrdersByContractorId(Guid contractorId)
    {
        var result = await _mediator.Send(new GetWorkOrdersByContractorIdQuery(contractorId));
        return Ok(result);
    }

    /// <summary>
    /// Gets all work orders assigned to the current contractor.
    /// </summary>
    /// <returns>List of work orders assigned to the contractor.</returns>
    [HttpGet("assigned")]
    [Authorize(Roles = $"{Roles.Contractor},{Roles.Admin}")]
    public async Task<ActionResult<List<WorkOrderDto>>> GetAssignedWorkOrders()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new GetWorkOrdersByContractorIdQuery(userId.Value));
        return Ok(result);
    }

    /// <summary>
    /// Gets all work orders for properties owned by an owner.
    /// </summary>
    /// <param name="ownerId">The owner ID.</param>
    /// <returns>List of work orders for properties owned by the owner.</returns>
    [HttpGet("owner/{ownerId}")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Admin}")]
    public async Task<ActionResult<List<WorkOrderDto>>> GetWorkOrdersByOwnerId(Guid ownerId)
    {
        var result = await _mediator.Send(new GetWorkOrdersByOwnerIdQuery(ownerId));
        return Ok(result);
    }

    /// <summary>
    /// Approves a work order.
    /// </summary>
    /// <param name="id">The work order ID.</param>
    /// <returns>The approved work order.</returns>
    [HttpPut("{id}/approve")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Admin}")]
    public async Task<ActionResult<WorkOrderDto>> ApproveWorkOrder(Guid id)
    {
        try
        {
            var command = new ApproveWorkOrderCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Rejects a work order.
    /// </summary>
    /// <param name="id">The work order ID.</param>
    /// <param name="request">The rejection request with reason.</param>
    /// <returns>The rejected work order.</returns>
    [HttpPut("{id}/reject")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Admin}")]
    public async Task<ActionResult<WorkOrderDto>> RejectWorkOrder(Guid id, [FromBody] RejectWorkOrderRequest? request = null)
    {
        try
        {
            var command = new RejectWorkOrderCommand(id, request?.Reason);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Assigns a work order to a contractor.
    /// </summary>
    /// <param name="id">The work order ID.</param>
    /// <param name="request">The assignment request with contractor ID.</param>
    /// <returns>The assigned work order.</returns>
    [HttpPut("{id}/assign")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Admin}")]
    public async Task<ActionResult<WorkOrderDto>> AssignWorkOrder(Guid id, [FromBody] AssignWorkOrderRequest request)
    {
        try
        {
            var command = new AssignWorkOrderCommand(id, request.ContractorId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Starts work on a work order.
    /// </summary>
    /// <param name="id">The work order ID.</param>
    /// <returns>The work order with started status.</returns>
    [HttpPut("{id}/start")]
    [Authorize(Roles = $"{Roles.Contractor},{Roles.Admin}")]
    public async Task<ActionResult<WorkOrderDto>> StartWorkOrder(Guid id)
    {
        try
        {
            var command = new StartWorkOrderCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Completes a work order.
    /// </summary>
    /// <param name="id">The work order ID.</param>
    /// <param name="request">The completion request with optional cost and notes.</param>
    /// <returns>The completed work order.</returns>
    [HttpPut("{id}/complete")]
    [Authorize(Roles = $"{Roles.Contractor},{Roles.Admin}")]
    public async Task<ActionResult<WorkOrderDto>> CompleteWorkOrder(Guid id, [FromBody] CompleteWorkOrderRequest? request = null)
    {
        try
        {
            var command = new CompleteWorkOrderCommand(id, request?.ActualCost, request?.Notes);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates a work order.
    /// </summary>
    /// <param name="id">The work order ID.</param>
    /// <param name="updateData">The update data.</param>
    /// <returns>The updated work order.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<WorkOrderDto>> UpdateWorkOrder(Guid id, [FromBody] UpdateWorkOrderDto updateData)
    {
        try
        {
            var command = new UpdateWorkOrderCommand(id, updateData);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancels a work order.
    /// </summary>
    /// <param name="id">The work order ID.</param>
    /// <param name="request">The cancellation request with optional reason.</param>
    /// <returns>The cancelled work order.</returns>
    [HttpPut("{id}/cancel")]
    public async Task<ActionResult<WorkOrderDto>> CancelWorkOrder(Guid id, [FromBody] CancelWorkOrderRequest? request = null)
    {
        try
        {
            var command = new CancelWorkOrderCommand(id, request?.Reason);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

/// <summary>
/// Request model for rejecting a work order.
/// </summary>
public class RejectWorkOrderRequest
{
    /// <summary>
    /// Gets or sets the rejection reason.
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Request model for assigning a work order.
/// </summary>
public class AssignWorkOrderRequest
{
    /// <summary>
    /// Gets or sets the contractor ID.
    /// </summary>
    public Guid ContractorId { get; set; }
}

/// <summary>
/// Request model for completing a work order.
/// </summary>
public class CompleteWorkOrderRequest
{
    /// <summary>
    /// Gets or sets the actual cost.
    /// </summary>
    public decimal? ActualCost { get; set; }

    /// <summary>
    /// Gets or sets the completion notes.
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request model for cancelling a work order.
/// </summary>
public class CancelWorkOrderRequest
{
    /// <summary>
    /// Gets or sets the cancellation reason.
    /// </summary>
    public string? Reason { get; set; }
}
