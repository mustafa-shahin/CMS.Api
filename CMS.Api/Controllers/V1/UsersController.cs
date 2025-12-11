using Asp.Versioning;
using CMS.Application.Common.Models;
using CMS.Application.Features.Users.Commands.ActivateUser;
using CMS.Application.Features.Users.Commands.CreateUser;
using CMS.Application.Features.Users.Commands.DeactivateUser;
using CMS.Application.Features.Users.Commands.DeleteUser;
using CMS.Application.Features.Users.Commands.UpdateUser;
using CMS.Application.Features.Users.DTOs;
using CMS.Application.Features.Users.Queries;
using CMS.Application.Common.Models.Search;
using CMS.Domain.Constants;
using CMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Api.Controllers.V1;

/// <summary>
/// User management controller for admin users.
/// Provides CRUD operations and user activation/deactivation.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Authorize(Policy = Permissions.CanManageUsers)]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a paginated list of users with optional filtering and sorting.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page (max 100).</param>
    /// <param name="searchTerm">Optional search term for email/name.</param>
    /// <param name="roleFilter">Optional role filter.</param>
    /// <param name="isActiveFilter">Optional active status filter.</param>
    /// <param name="sortBy">Property to sort by.</param>
    /// <param name="sortDescending">Sort in descending order.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of users.</returns>
    /// <response code="200">Users retrieved successfully.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized to manage users.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<UserListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] UserRole? roleFilter = null,
        [FromQuery] bool? isActiveFilter = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersWithPaginationQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            RoleFilter = roleFilter,
            IsActiveFilter = isActiveFilter,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(PaginatedResponse<UserListDto>.FromPaginatedList(result));
    }

    /// <summary>
    /// Advanced search for users with full-text search, filtering, sorting, and paging.
    /// </summary>
    /// <param name="request">Search request with filters, sorts, and search term.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search result with users.</returns>
    /// <response code="200">Users retrieved successfully.</response>
    /// <response code="400">Invalid search request.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized to manage users.</response>
    [HttpPost("search")]
    [ProducesResponseType(typeof(ApiResponse<SearchResult<UserListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchUsers(
        [FromBody] SearchUsersQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(ApiResponse<SearchResult<UserListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets a specific user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User details.</returns>
    /// <response code="200">User found.</response>
    /// <response code="404">User not found.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized to manage users.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUser(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Creates a new user (admin only).
    /// </summary>
    /// <param name="command">User creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created user details.</returns>
    /// <response code="201">User created successfully.</response>
    /// <response code="400">Validation error or email already exists.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized to manage users.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(
            nameof(GetUser),
            new { id = result.Id },
            ApiResponse<UserDto>.SuccessResponse(result, "User created successfully."));
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="id">The user ID to update.</param>
    /// <param name="command">Updated user details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated user details.</returns>
    /// <response code="200">User updated successfully.</response>
    /// <response code="400">Validation error or ID mismatch.</response>
    /// <response code="404">User not found.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized to manage users.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(
        [FromRoute] int id,
        [FromBody] UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("Route ID and body ID must match."));
        }

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result, "User updated successfully."));
    }

    /// <summary>
    /// Deletes a user.
    /// </summary>
    /// <param name="id">The user ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">User deleted successfully.</response>
    /// <response code="400">Cannot delete self or last admin.</response>
    /// <response code="404">User not found.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized to manage users.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Activates a deactivated user.
    /// </summary>
    /// <param name="id">The user ID to activate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">User activated successfully.</response>
    /// <response code="404">User not found.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized to manage users.</response>
    [HttpPost("{id:int}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateUser(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ActivateUserCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deactivates an active user.
    /// </summary>
    /// <param name="id">The user ID to deactivate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">User deactivated successfully.</response>
    /// <response code="400">Cannot deactivate self.</response>
    /// <response code="404">User not found.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Not authorized to manage users.</response>
    [HttpPost("{id:int}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateUser(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeactivateUserCommand(id), cancellationToken);
        return NoContent();
    }
}