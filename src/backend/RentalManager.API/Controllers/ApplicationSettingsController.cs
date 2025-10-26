// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManager.Application.DTOs;
using RentalManager.Application.Queries;

namespace RentalManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApplicationSettingsDto>> GetSettings()
    {
        var query = new GetApplicationSettingsQuery();
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}

