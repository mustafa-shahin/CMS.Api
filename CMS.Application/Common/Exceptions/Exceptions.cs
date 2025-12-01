namespace CMS.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    public ValidationException(string propertyName, string errorMessage)
        : this()
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, [errorMessage] }
        };
    }

    public IDictionary<string, string[]> Errors { get; }
}

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException()
        : base("The requested resource was not found.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}

/// <summary>
/// Exception thrown when the user is not authenticated.
/// </summary>
public sealed class UnauthorizedException : Exception
{
    public string ErrorCode { get; }

    public UnauthorizedException()
        : base("Authentication is required.")
    {
        ErrorCode = Shared.Constants.ErrorCodes.Unauthorized;
    }

    public UnauthorizedException(string message, string errorCode = "AUTH_1009")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception thrown when the user lacks permission to perform an action.
/// </summary>
public sealed class ForbiddenAccessException : Exception
{
    public string ErrorCode { get; }

    public ForbiddenAccessException()
        : base("You do not have permission to perform this action.")
    {
        ErrorCode = Shared.Constants.ErrorCodes.Forbidden;
    }

    public ForbiddenAccessException(string message, string errorCode = "AUTH_1010")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception thrown for business rule violations.
/// </summary>
public sealed class BusinessRuleException : Exception
{
    public string ErrorCode { get; }

    public BusinessRuleException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}