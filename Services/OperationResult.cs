namespace HealthCare_Portal.Services;

public sealed class OperationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public static OperationResult Ok(string message = "") => new() { Success = true, Message = message };
    public static OperationResult Fail(string message, params string[] errors) => new() { Message = message, Errors = errors.Length == 0 ? new[] { message } : errors };
}

public sealed class OperationResult<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
    public T? Data { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public static OperationResult<T> Ok(T data, string message = "") => new() { Success = true, Data = data, Message = message };
    public static OperationResult<T> Fail(string message, params string[] errors) => new() { Message = message, Errors = errors.Length == 0 ? new[] { message } : errors };
}
