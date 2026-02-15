namespace CampusRooms.Api.Services;

public sealed class ServiceResult<T>
{
    private ServiceResult(bool success, bool notFound, T? value, string? error)
    {
        Success = success;
        NotFound = notFound;
        Value = value;
        Error = error;
    }

    public bool Success { get; }
    public bool NotFound { get; }
    public T? Value { get; }
    public string? Error { get; }

    public static ServiceResult<T> Ok(T value) => new(true, false, value, null);
    public static ServiceResult<T> Fail(string error) => new(false, false, default, error);
    public static ServiceResult<T> NotFoundResult() => new(false, true, default, null);
}
