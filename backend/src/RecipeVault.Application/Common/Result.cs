namespace RecipeVault.Application.Common;

public enum ResultStatus
{
    Ok,
    Invalid,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
}

/// <summary>
/// The outcome of an application operation. Expected failures (bad input, missing record,
/// not the owner) are returned as values rather than thrown, so callers have to handle them.
/// </summary>
public sealed class Result<T>
{
    private Result(ResultStatus status, T? value, IReadOnlyList<string> errors)
    {
        Status = status;
        Value = value;
        Errors = errors;
    }

    public ResultStatus Status { get; }
    public T? Value { get; }
    public IReadOnlyList<string> Errors { get; }
    public bool IsOk => Status == ResultStatus.Ok;

    public static Result<T> Ok(T value) => new(ResultStatus.Ok, value, []);
    public static Result<T> Invalid(IReadOnlyList<string> errors) => new(ResultStatus.Invalid, default, errors);
    public static Result<T> Unauthorized(string error) => new(ResultStatus.Unauthorized, default, [error]);
    public static Result<T> Forbidden(string error) => new(ResultStatus.Forbidden, default, [error]);
    public static Result<T> NotFound(string error) => new(ResultStatus.NotFound, default, [error]);
    public static Result<T> Conflict(string error) => new(ResultStatus.Conflict, default, [error]);
}
