using Asp.Versioning;
using CMS.Application.Common.Models;
using CMS.Application.Features.Media.Commands.DeleteImage;
using CMS.Application.Features.Media.Commands.UpdateImage;
using CMS.Application.Features.Media.Commands.UploadImage;
using CMS.Application.Features.Media.DTOs;
using CMS.Application.Features.Media.Queries.GetImage;
using CMS.Application.Features.Media.Queries.GetImagesWithPagination;
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
    /// Get all images with pagination.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 12)</param>
    /// <param name="searchTerm">Optional search term</param>
    /// <param name="folderId">Optional folder filter</param>
    /// <param name="contentType">Optional content type filter</param>
    /// <param name="sortBy">Sort field (name, size, createdAt)</param>
    /// <param name="sortDescending">Sort direction (default: true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of images</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedList<ImageListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedList<ImageListDto>>> GetImages(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? folderId = null,
        [FromQuery] string? contentType = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetImagesWithPaginationQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            FolderId = folderId,
            ContentType = contentType,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
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
    /// Update image metadata.
    /// </summary>
    /// <param name="id">Image ID</param>
    /// <param name="command">Update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated image information</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ImageListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImageListDto>> UpdateImage(
        int id,
        [FromBody] UpdateImageCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest("ID in URL does not match ID in request body.");
        }

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete an image.
    /// </summary>
    /// <param name="id">Image ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(
        int id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteImageCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get image data by ID.
    /// </summary>
    /// <param name="id">Image ID</param>
    /// <param name="variant">Image variant (original, thumbnail, medium)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Image file</returns>
    [HttpGet("{id}/file")]
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

    /// <summary>
    /// Download an image file.
    /// </summary>
    /// <param name="id">Image ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Image file as download attachment</returns>
    [HttpGet("{id}/download")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadImage(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetImageQuery
        {
            ImageId = id,
            Variant = ImageVariant.Original
        };

        var response = await _mediator.Send(query, cancellationToken);
        if (response == null)
        {
            return NotFound();
        }

        // Set Content-Disposition to attachment for download
        Response.Headers.ContentDisposition = $"attachment; filename=\"{response.FileName}\"";

        return File(response.Data, response.ContentType, response.FileName);
    }
}

