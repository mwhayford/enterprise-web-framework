// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Queries;

namespace RentalManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropertiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<PropertyListDto>>> GetAvailableProperties([FromQuery] GetAvailablePropertiesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PropertyDto>> GetPropertyById(Guid id)
    {
        var result = await _mediator.Send(new GetPropertyByIdQuery(id));

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PropertyDto>> CreateProperty([FromBody] CreatePropertyDto propertyData)
    {
        var command = new CreatePropertyCommand(propertyData);
        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetPropertyById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<PropertyDto>> UpdateProperty(Guid id, [FromBody] CreatePropertyDto propertyData)
    {
        var command = new UpdatePropertyCommand(id, propertyData);
        var result = await _mediator.Send(command);

        return Ok(result);
    }
}
