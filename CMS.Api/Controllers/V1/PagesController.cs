using Asp.Versioning;
using CMS.Application.Common.Models;
using CMS.Application.Common.Models.Search;
using CMS.Application.Features.Pages.DTOs;
using CMS.Application.Features.Pages.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Api.Controllers.V1;

/// <summary>
/// Controller for managing pages.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
[Authorize]
public sealed class PagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PagesController> _logger;

    public PagesController(IMediator mediator, ILogger<PagesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Advanced search for pages with full-text search, filtering, sorting, and paging.
    /// </summary>
    /// <param name="request">Search request with filters, sorts, and search term.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search result with pages.</returns>
    /// <response code="200">Pages retrieved successfully.</response>
    /// <response code="400">Invalid search request.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpPost("search")]
    [ProducesResponseType(typeof(ApiResponse<SearchResult<PageListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SearchPages(
        [FromBody] SearchPagesQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(ApiResponse<SearchResult<PageListDto>>.SuccessResponse(result));
    }
}
