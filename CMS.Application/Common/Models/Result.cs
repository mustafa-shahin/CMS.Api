namespace CMS.Application.Common.Models;

/// <summary>
/// Represents the result of an operation that may fail.
/// </summary>
public class Result
{
    protected Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    public bool Succeeded { get; }
    public string[] Errors { get; }
    public string? ErrorCode { get; init; }

    public static Result Success() => new(true, []);

    public static Result Failure(params string[] errors) => new(false, errors);

    public static Result Failure(string errorCode, params string[] errors) =>
        new(false, errors) { ErrorCode = errorCode };
}

/// <summary>
/// Represents the result of an operation that may fail and returns a value on success.
/// </summary>
/// <typeparam name="T">The type of value returned on success.</typeparam>
public class Result<T> : Result
{
    private readonly T? _value;

    protected Result(T? value, bool succeeded, IEnumerable<string> errors)
        : base(succeeded, errors)
    {
        _value = value;
    }

    public T Value => Succeeded
        ? _value!
        : throw new InvalidOperationException("Cannot access Value when the operation failed.");

    public static Result<T> Success(T value) => new(value, true, []);

    public new static Result<T> Failure(params string[] errors) => new(default, false, errors);

    public static Result<T> Failure(string errorCode, params string[] errors) =>
        new(default, false, errors) { ErrorCode = errorCode };

    public static implicit operator Result<T>(T value) => Success(value);
}