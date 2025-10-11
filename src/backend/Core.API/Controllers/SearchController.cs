// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Application.Commands;
using Core.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<SearchResultDto<object>>> Search(
        [FromQuery] string query,
        [FromQuery] string index = "core-index",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var command = new SearchCommand<object>
        {
            Query = query,
            Index = index,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("index")]
    public async Task<ActionResult> IndexDocument(
        [FromBody] object document,
        [FromQuery] string index = "core-index",
        [FromQuery] string? id = null)
    {
        var command = new IndexDocumentCommand<object>
        {
            Document = document,
            Index = index,
            Id = id
        };

        await _mediator.Send(command);
        return Ok();
    }

    [HttpPost("users")]
    public async Task<ActionResult<SearchResultDto<UserDto>>> SearchUsers(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var command = new SearchCommand<UserDto>
        {
            Query = query,
            Index = "users",
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("payments")]
    public async Task<ActionResult<SearchResultDto<PaymentDto>>> SearchPayments(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var command = new SearchCommand<PaymentDto>
        {
            Query = query,
            Index = "payments",
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
