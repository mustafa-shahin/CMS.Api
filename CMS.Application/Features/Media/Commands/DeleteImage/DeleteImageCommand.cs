using CMS.Application.Common.Exceptions;
using CMS.Application.Common.Interfaces;
using CMS.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Media.Commands.DeleteImage;

/// <summary>
/// Command to delete an image.
/// </summary>
public sealed record DeleteImageCommand(int Id) : IRequest<Unit>;

/// <summary>
/// Validator for DeleteImageCommand.
/// </summary>
public sealed class DeleteImageCommandValidator : AbstractValidator<DeleteImageCommand>
{
    public DeleteImageCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Image ID must be greater than 0.");
    }
}

/// <summary>
/// Handler for DeleteImageCommand.
/// </summary>
public sealed class DeleteImageCommandHandler : IRequestHandler<DeleteImageCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteImageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeleteImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _context.Images
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (image is null)
        {
            throw new NotFoundException("Image", request.Id);
        }

        // Store info for audit log before deletion
        var imageInfo = $"Deleted image: {image.OriginalName} ({image.FileName})";

        // Remove the image
        _context.Images.Remove(image);

        // Log deletion
        var auditLog = AuditLog.Create(
            _currentUserService.UserId,
            "DeleteImage",
            nameof(ImageEntity),
            request.Id,
            ipAddress: _currentUserService.IpAddress,
            userAgent: _currentUserService.UserAgent,
            additionalInfo: imageInfo);

        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
