using CMS.Api.Attributes;
using Asp.Versioning;
using CMS.Application.Common.Models;
using CMS.Application.Features.LayoutCustomization.Commands.UpdateLayoutSettings;
using CMS.Application.Features.LayoutCustomization.DTOs;
using CMS.Application.Features.LayoutCustomization.Queries.GetLayoutSettings;
using CMS.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Api.Controllers.V1;

/// <summary>
/// Controller for managing layout customization settings including header, footer, and spacing.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/customization/layout")]
[ApiController]
[Authorize]
public sealed class LayoutCustomizationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LayoutCustomizationController> _logger;

    public LayoutCustomizationController(IMediator mediator, ILogger<LayoutCustomizationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get current layout settings
    /// </summary>
    /// <remarks>
    /// Retrieves layout configuration including header, footer, and spacing options.
    /// If no custom settings exist, returns default values.
    /// Available to all authenticated users.
    /// </remarks>
    /// <returns>Current layout settings</returns>
    /// <response code="200">Layout settings retrieved successfully</response>
    /// <response code="401">Unauthorized - Authentication required</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<LayoutSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLayoutSettings(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/v1/customization/layout - Retrieving layout settings");

        var query = new GetLayoutSettingsQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<LayoutSettingsDto>
        {
            Success = true,
            Data = result,
            Message = "Layout settings retrieved successfully"
        });
    }

    /// <summary>
    /// Update layout settings
    /// </summary>
    /// <remarks>
    /// Updates the complete layout configuration.
    /// Requires Admin or Developer role.
    ///
    /// **Header Options:**
    /// - Template: Minimal, Standard, or Full
    /// - Logo Placement: Left, Center, or Right
    /// - Show Search: Toggle search bar visibility
    /// - Sticky Header: Toggle sticky positioning
    ///
    /// **Footer Options:**
    /// - Template: Minimal, Standard, or Full
    /// - Column Count: 1-4 columns
    /// - Show Social Links: Toggle social media icons
    /// - Show Newsletter: Toggle newsletter signup
    ///
    /// **Spacing Configuration:**
    /// - Container Max Width: 640-1920 pixels
    /// - Section Padding: 1-8 rem
    /// - Component Gap: 0.5-4 rem
    /// </remarks>
    /// <param name="command">Layout settings to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated layout settings</returns>
    /// <response code="200">Layout settings updated successfully</response>
    /// <response code="400">Invalid input - Validation errors</response>
    /// <response code="401">Unauthorized - Authentication required</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="429">Too Many Requests - Rate limit exceeded</response>
    [HttpPut]
    [Authorize(Policy = Permissions.CanManageConfiguration)]
    [RateLimit(Requests = 20, PerMinutes = 1)]
    [ProducesResponseType(typeof(ApiResponse<LayoutSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateLayoutSettings([FromBody] UpdateLayoutSettingsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("PUT /api/v1/customization/layout - Updating layout settings");

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<LayoutSettingsDto>
        {
            Success = true,
            Data = result,
            Message = "Layout settings updated successfully"
        });
    }
}
