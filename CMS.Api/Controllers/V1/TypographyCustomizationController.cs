using Asp.Versioning;
using CMS.Application.Common.Models;
using CMS.Application.Features.TypographyCustomization.Commands.UpdateTypographySettings;
using CMS.Application.Features.TypographyCustomization.DTOs;
using CMS.Application.Features.TypographyCustomization.Queries.GetTypographySettings;
using CMS.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Api.Controllers.V1;

/// <summary>
/// Controller for managing typography customization settings including fonts and text styles.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/customization/typography")]
[ApiController]
[Authorize]
public sealed class TypographyCustomizationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TypographyCustomizationController> _logger;

    public TypographyCustomizationController(IMediator mediator, ILogger<TypographyCustomizationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get current typography settings
    /// </summary>
    /// <remarks>
    /// Retrieves typography configuration including font families and text styles.
    /// Includes settings for headings (H1-H6), body text, and special text types.
    /// If no custom settings exist, returns default values with Inter font.
    /// Available to all authenticated users.
    /// </remarks>
    /// <returns>Current typography settings</returns>
    /// <response code="200">Typography settings retrieved successfully</response>
    /// <response code="401">Unauthorized - Authentication required</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<TypographySettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTypographySettings(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/v1/customization/typography - Retrieving typography settings");

        var query = new GetTypographySettingsQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<TypographySettingsDto>
        {
            Success = true,
            Data = result,
            Message = "Typography settings retrieved successfully"
        });
    }

    /// <summary>
    /// Update typography settings
    /// </summary>
    /// <remarks>
    /// Updates the complete typography configuration.
    /// Requires Admin or Developer role.
    ///
    /// **Font Families:**
    /// - Primary: Used for headings
    /// - Secondary: Used for body text
    /// - Mono: Used for code blocks
    ///
    /// **Text Styles:**
    /// - Heading1-6: Different heading levels
    /// - BodyLarge/Medium/Small: Body text variations
    /// - Caption, Overline, ButtonText, LinkText: Special text styles
    ///
    /// **Validation Rules:**
    /// - Font size: 0.5-10 rem
    /// - Font weight: 100-900 (increments of 100)
    /// - Line height: 0.8-3
    /// - Letter spacing: -0.1 to 0.5 em (optional)
    /// </remarks>
    /// <param name="command">Typography settings to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated typography settings</returns>
    /// <response code="200">Typography settings updated successfully</response>
    /// <response code="400">Invalid input - Validation errors</response>
    /// <response code="401">Unauthorized - Authentication required</response>
    /// <response code="403">Forbidden - Insufficient permissions</response>
    [HttpPut]
    [Authorize(Policy = Permissions.CanManageConfiguration)]
    [ProducesResponseType(typeof(ApiResponse<TypographySettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateTypographySettings([FromBody] UpdateTypographySettingsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("PUT /api/v1/customization/typography - Updating typography settings");

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<TypographySettingsDto>
        {
            Success = true,
            Data = result,
            Message = "Typography settings updated successfully"
        });
    }
}
