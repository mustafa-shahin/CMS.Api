using Asp.Versioning;
using CMS.Application.Common.Models;
using CMS.Application.Common.Models.Search;
using CMS.Application.Features.Media.Commands.UploadImage;
using CMS.Application.Features.Media.DTOs;
using CMS.Application.Features.Media.Queries;
using CMS.Application.Features.Media.Queries.GetImage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Api.Controllers.V1;

/// <summary>
/// Controller for managing images.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
public sealed class ImagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(IMediator mediator, ILogger<ImagesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Advanced search for images with full-text search, filtering, sorting, and paging.
    /// </summary>
    /// <param name="request">Search request with filters, sorts, and search term.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search result with images.</returns>
    /// <response code="200">Images retrieved successfully.</response>
    /// <response code="400">Invalid search request.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpPost("search")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SearchResult<ImageListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SearchImages(
        [FromBody] SearchImagesQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(ApiResponse<SearchResult<ImageListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Upload a new image.
    /// </summary>
    /// <param name="file">Image file to upload</param>
    /// <param name="folderId">Optional folder ID</param>
    /// <param name="altText">Optional alt text</param>
    /// <param name="caption">Optional caption</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Uploaded image information</returns>
    [HttpPost("upload")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB limit
    [ProducesResponseType(typeof(UploadImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UploadImageResponse>> UploadImage(
        IFormFile file,
        [FromForm] int? folderId,
        [FromForm] string? altText,
        [FromForm] string? caption,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file provided.");
        }

        // Validate file type
        var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return BadRequest($"Invalid file type. Allowed types: {string.Join(", ", allowedContentTypes)}");
        }

        // Validate file size (10 MB max)
        const long maxFileSize = 10 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            return BadRequest($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)} MB.");
        }

        try
        {
            // Read file data
            byte[] fileData;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream, cancellationToken);
                fileData = memoryStream.ToArray();
            }

            // Create command
            var command = new UploadImageCommand
            {
                FileData = fileData,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FolderId = folderId,
                AltText = altText,
                Caption = caption,
                GenerateVariants = true
            };

            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid image upload attempt");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while uploading the image.");
        }
    }

    /// <summary>
    /// Get image data by ID.
    /// </summary>
    /// <param name="id">Image ID</param>
    /// <param name="variant">Image variant (original, thumbnail, medium)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Image file</returns>
    [HttpGet("{id}")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)] // Cache for 1 hour
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImage(
        int id,
        [FromQuery] ImageVariant variant = ImageVariant.Original,
        CancellationToken cancellationToken = default)
    {
        var query = new GetImageQuery
        {
            ImageId = id,
            Variant = variant
        };

        var response = await _mediator.Send(query, cancellationToken);
        if (response == null)
        {
            return NotFound();
        }

        // Set cache headers
        if (response.LastModified.HasValue)
        {
            Response.Headers.LastModified = response.LastModified.Value.ToString("R");
        }

        Response.Headers.CacheControl = "public, max-age=3600";

        return File(response.Data, response.ContentType, response.FileName);
    }

    /// <summary>
    /// Get image thumbnail by ID.
    /// </summary>
    [HttpGet("{id}/thumbnail")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetThumbnail(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await GetImage(id, ImageVariant.Thumbnail, cancellationToken);
    }

    /// <summary>
    /// Get medium-sized image by ID.
    /// </summary>
    [HttpGet("{id}/medium")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMedium(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await GetImage(id, ImageVariant.Medium, cancellationToken);
    }
}
