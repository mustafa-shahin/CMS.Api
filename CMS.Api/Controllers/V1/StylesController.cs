using Asp.Versioning;
using CMS.Api.Attributes;
using CMS.Application.Common.Models;
using CMS.Application.Features.Styles.Commands.UpdateStyleSettings;
using CMS.Application.Features.Styles.DTOs;
using CMS.Application.Features.Styles.Queries.GetStyleSettings;
using CMS.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Api.Controllers.V1;

/// <summary>
/// Controller for managing style settings (navbar and footer colors)
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public sealed class StylesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StylesController> _logger;

    public StylesController(IMediator mediator, ILogger<StylesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get current style settings
    /// </summary>
    /// <remarks>
    /// Retrieves the current navbar and footer background colors.
    /// If no custom settings exist, returns default values.
    /// Available to all authenticated users.
    /// </remarks>
    /// <returns>Current style settings</returns>
    /// <response code="200">Style settings retrieved successfully</response>
    /// <response code="401">Unauthorized - Authentication required</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<StyleSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStyleSettings(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/v1/styles - Retrieving style settings");

        var query = new GetStyleSettingsQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<StyleSettingsDto>
        {
            Success = true,
            Data = result,
            Message = "Style settings retrieved successfully"
        });
    }

    /// <summary>
    /// Update style settings
    /// </summary>
    /// <remarks>
    /// Updates the navbar and footer background colors.
    /// Requires Admin or Developer role.
    /// Colors must be in hex format (e.g., #1f2937).
    /// </remarks>
    /// <param name="command">Style settings to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated style settings</returns>
    /// <response code="200">Style settings updated successfully</response>
    /// <response code="400">Invalid input - Validation errors</response>
    /// <response code="401">Unauthorized - Authentication required</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="429">Too Many Requests - Rate limit exceeded</response>
    [HttpPut]
    [Authorize(Policy = Permissions.CanManageConfiguration)]
    [RateLimit(Requests = 20, PerMinutes = 1)] // Allow 20 updates per minute
    [ProducesResponseType(typeof(ApiResponse<StyleSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateStyleSettings(
        [FromBody] UpdateStyleSettingsCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "PUT /api/v1/styles - Updating style settings: NavbarBackground={NavbarBackground}, FooterBackground={FooterBackground}",
            command.NavbarBackground, command.FooterBackground);

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<StyleSettingsDto>
        {
            Success = true,
            Data = result,
            Message = "Style settings updated successfully"
        });
    }
}
