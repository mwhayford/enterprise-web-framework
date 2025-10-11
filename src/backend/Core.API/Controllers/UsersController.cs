// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Core.Application.Commands;
using Core.Application.Queries;
using MediatR;

namespace Core.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var user = await _mediator.Send(new GetUserByIdQuery { UserId = userGuid });
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserProfileCommand command)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        command = command with { UserId = userGuid };
        var user = await _mediator.Send(command);
        return Ok(user);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null, [FromQuery] bool? isActive = null)
    {
        var users = await _mediator.Send(new GetUsersQuery 
        { 
            Page = page, 
            PageSize = pageSize,
            SearchTerm = searchTerm,
            IsActive = isActive
        });
        return Ok(users);
    }

    [HttpGet("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var user = await _mediator.Send(new GetUserByIdQuery { UserId = userId });
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPut("{userId}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateUser(Guid userId)
    {
        var result = await _mediator.Send(new DeactivateUserCommand { UserId = userId });
        if (!result)
        {
            return NotFound();
        }
        return Ok(new { Message = "User deactivated successfully" });
    }

    [HttpPut("{userId}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActivateUser(Guid userId)
    {
        var result = await _mediator.Send(new ActivateUserCommand { UserId = userId });
        if (!result)
        {
            return NotFound();
        }
        return Ok(new { Message = "User activated successfully" });
    }
}
