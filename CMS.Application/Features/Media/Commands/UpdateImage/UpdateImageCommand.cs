using CMS.Application.Common.Exceptions;
using CMS.Application.Common.Interfaces;
using CMS.Application.Features.Media.DTOs;
using CMS.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Media.Commands.UpdateImage;

/// <summary>
/// Command to update image metadata.
/// </summary>
public sealed record UpdateImageCommand : IRequest<ImageListDto>
{
    public int Id { get; init; }
    public string? AltText { get; init; }
    public string? Caption { get; init; }
    public int? FolderId { get; init; }
}

/// <summary>
/// Validator for UpdateImageCommand.
/// </summary>
public sealed class UpdateImageCommandValidator : AbstractValidator<UpdateImageCommand>
{
    public UpdateImageCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Image ID must be greater than 0.");

        RuleFor(x => x.AltText)
            .MaximumLength(500).WithMessage("Alt text must not exceed 500 characters.")
            .When(x => x.AltText != null);

        RuleFor(x => x.Caption)
            .MaximumLength(2000).WithMessage("Caption must not exceed 2000 characters.")
            .When(x => x.Caption != null);
    }
}

/// <summary>
/// Handler for UpdateImageCommand.
/// </summary>
public sealed class UpdateImageCommandHandler : IRequestHandler<UpdateImageCommand, ImageListDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateImageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ImageListDto> Handle(UpdateImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _context.Images
            .Include(i => i.UploadedByUser)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (image is null)
        {
            throw new NotFoundException("Image", request.Id);
        }

        // Update metadata
        image.UpdateAltText(request.AltText);
        image.UpdateCaption(request.Caption);

        // Move to folder if specified
        if (request.FolderId.HasValue)
        {
            image.MoveToFolder(request.FolderId.Value);
        }

        // Log update
        var auditLog = AuditLog.Create(
            _currentUserService.UserId,
            "UpdateImage",
            nameof(ImageEntity),
            image.Id,
            ipAddress: _currentUserService.IpAddress,
            userAgent: _currentUserService.UserAgent);

        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        return new ImageListDto
        {
            Id = image.Id,
            FileName = image.FileName,
            OriginalName = image.OriginalName,
            ContentType = image.ContentType,
            Size = image.Size,
            Width = image.Width,
            Height = image.Height,
            AltText = image.AltText,
            Caption = image.Caption,
            HasThumbnail = image.ThumbnailData != null,
            HasMediumVersion = image.MediumData != null,
            FolderId = image.FolderId,
            CreatedAt = image.CreatedAt,
            ModifiedAt = image.LastModifiedAt,
            UploadedByUserId = image.UploadedByUserId,
            UploadedByUserName = image.UploadedByUser.FirstName + " " + image.UploadedByUser.LastName
        };
    }
}
