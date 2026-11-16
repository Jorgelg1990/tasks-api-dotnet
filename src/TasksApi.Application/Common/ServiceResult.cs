namespace TasksApi.Application.Common;

public enum ResultKind
{
    Success,
    NotFound,
    Invalid
}

public sealed record ServiceResult<T>(ResultKind Kind, T? Value, string? Message)
{
    public static ServiceResult<T> Success(T value) => new(ResultKind.Success, value, null);

    public static ServiceResult<T> NotFound(string message) => new(ResultKind.NotFound, default, message);

    public static ServiceResult<T> Invalid(string message) => new(ResultKind.Invalid, default, message);
}
