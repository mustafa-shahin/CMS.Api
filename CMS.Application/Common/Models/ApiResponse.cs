namespace CMS.Application.Common.Models;

/// <summary>
/// Standard API response wrapper for all endpoints.
/// </summary>
/// <typeparam name="T">The type of data being returned.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>
    /// Indicates if the request was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The response data (null on failure).
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Error message if the request failed.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Error code for programmatic error handling.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string? message = null) =>
        new()
        {
            Success = true,
            Data = data,
            Message = message
        };

    /// <summary>
    /// Creates a failure response.
    /// </summary>
    public static ApiResponse<T> FailureResponse(string message, string? errorCode = null) =>
        new()
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode
        };
}

/// <summary>
/// Standard API error response for exception handling.
/// </summary>
public sealed class ApiErrorResponse
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public string ErrorCode { get; init; } = null!;
    public string Message { get; init; } = null!;
    public IDictionary<string, string[]>? Errors { get; init; }
    public string? Detail { get; init; }
    public string TraceId { get; init; } = null!;
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// Paginated API response for list endpoints.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public sealed class PaginatedResponse<T>
{
    /// <summary>
    /// Indicates if the request was successful.
    /// </summary>
    public bool Success { get; init; } = true;

    /// <summary>
    /// The items on the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// The page size.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage { get; init; }

    /// <summary>
    /// Whether there is a next page.
    /// </summary>
    public bool HasNextPage { get; init; }

    /// <summary>
    /// Creates a paginated response from a PaginatedList.
    /// </summary>
    public static PaginatedResponse<T> FromPaginatedList(PaginatedList<T> list) =>
        new()
        {
            Items = list.Items,
            PageNumber = list.PageNumber,
            PageSize = list.PageSize,
            TotalPages = list.TotalPages,
            TotalCount = list.TotalCount,
            HasPreviousPage = list.HasPreviousPage,
            HasNextPage = list.HasNextPage
        };
}