using Asp.Versioning;
using CMS.Application.Common.Models;
using CMS.Application.Features.Auth;
using CMS.Application.Features.Auth.Commands.Login;
using CMS.Application.Features.Auth.Commands.Logout;
using CMS.Application.Features.Auth.Commands.RefreshTokens;
using CMS.Application.Features.Auth.Commands.Register;
using CMS.Application.Features.Users.DTOs;
using CMS.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CMS.Api.Controllers.V1;

/// <summary>
/// Authentication controller for login, registration, token management, and logout.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="command">Login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JWT access token, refresh token, and user information.</returns>
    /// <response code="200">Authentication successful.</response>
    /// <response code="401">Invalid credentials or account locked/inactive.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="429">Too many requests.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful."));
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="command">Registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JWT access token, refresh token, and user information.</returns>
    /// <response code="201">Registration successful.</response>
    /// <response code="400">Validation error or email already exists.</response>
    /// <response code="429">Too many requests.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<AuthResponse>.SuccessResponse(result, "Registration successful."));
    }

    /// <summary>
    /// Refreshes the JWT access token using a valid refresh token.
    /// Implements token rotation - the old refresh token is invalidated.
    /// </summary>
    /// <param name="command">The refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New access and refresh tokens.</returns>
    /// <response code="200">Token refresh successful.</response>
    /// <response code="401">Invalid, expired, or revoked refresh token.</response>
    /// <response code="429">Too many requests.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<TokenRefreshResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<TokenRefreshResponse>.SuccessResponse(result, "Token refreshed successfully."));
    }

    /// <summary>
    /// Logs out the current user by revoking their refresh token(s).
    /// </summary>
    /// <param name="command">Logout options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Logout successful.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current user information.</returns>
    /// <response code="200">User information retrieved successfully.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return Ok(ApiResponse<CurrentUserDto>.SuccessResponse(result));
    }
}