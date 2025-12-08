using Asp.Versioning;
using CMS.Api.Attributes;
using CMS.Application.Common.Models;
using CMS.Application.Features.ThemeCustomization.Commands.UpdateThemeSettings;
using CMS.Application.Features.ThemeCustomization.DTOs;
using CMS.Application.Features.ThemeCustomization.Queries.GetThemeSettings;
using CMS.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Api.Controllers.V1;

/// <summary>
/// Controller for managing theme customization settings including all color palettes.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/customization/theme")]
[ApiController]
[Authorize]
public sealed class ThemeCustomizationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ThemeCustomizationController> _logger;

    public ThemeCustomizationController(IMediator mediator, ILogger<ThemeCustomizationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get current theme settings
    /// </summary>
    /// <remarks>
    /// Retrieves the current theme configuration including brand, neutral, and semantic color palettes.
    /// Each color scheme includes base, light, dark, and contrast variants.
    /// If no custom settings exist, returns default values.
    /// Available to all authenticated users.
    /// </remarks>
    /// <returns>Current theme settings with all color palettes</returns>
    /// <response code="200">Theme settings retrieved successfully</response>
    /// <response code="401">Unauthorized - Authentication required</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ThemeSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetThemeSettings(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/v1/customization/theme - Retrieving theme settings");

        var query = new GetThemeSettingsQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<ThemeSettingsDto>
        {
            Success = true,
            Data = result,
            Message = "Theme settings retrieved successfully"
        });
    }

    /// <summary>
    /// Update theme settings
    /// </summary>
    /// <remarks>
    /// Updates the complete theme configuration including all color palettes.
    /// Requires Admin or Developer role.
    /// All colors must be in hex format (e.g., #3B82F6).
    /// Color variants (light, dark, contrast) are validated but can be customized.
    ///
    /// **Color Palettes:**
    /// - **Brand Palette**: Primary branding colors (primary, secondary, accent)
    /// - **Neutral Palette**: UI backgrounds and borders (light, medium, dark)
    /// - **Semantic Palette**: Feedback colors (success, warning, error)
    ///
    /// Each color scheme includes:
    /// - Base: Main color
    /// - Light: 20% lighter variant
    /// - Dark: 20% darker variant
    /// - Contrast: Auto-calculated for accessibility (black or white)
    /// </remarks>
    /// <param name="command">Theme settings to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated theme settings</returns>
    /// <response code="200">Theme settings updated successfully</response>
    /// <response code="400">Invalid input - Validation errors</response>
    /// <response code="401">Unauthorized - Authentication required</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    /// <response code="429">Too Many Requests - Rate limit exceeded</response>
    [HttpPut]
    [Authorize(Policy = Permissions.CanManageConfiguration)]
    [RateLimit(Requests = 20, PerMinutes = 1)]
    [ProducesResponseType(typeof(ApiResponse<ThemeSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateThemeSettings([FromBody] UpdateThemeSettingsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("PUT /api/v1/customization/theme - Updating theme settings");

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<ThemeSettingsDto>
        {
            Success = true,
            Data = result,
            Message = "Theme settings updated successfully"
        });
    }
}
