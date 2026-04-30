namespace Togo.Application.Tutors;

public enum ApplicationResultType
{
    Success,
    NotFound,
    ValidationError,
    Conflict,
    Error
}

public class ApplicationResult<T>
{
    private ApplicationResult(ApplicationResultType type, T? data = default, string? error = null)
    {
        Type = type;
        Data = data;
        Error = error;
    }

    public ApplicationResultType Type { get; }
    public T? Data { get; }
    public string? Error { get; }

    public bool IsSuccess => Type == ApplicationResultType.Success;

    public static ApplicationResult<T> Success(T data) => new(ApplicationResultType.Success, data);
    public static ApplicationResult<T> NotFound(string error) => new(ApplicationResultType.NotFound, error: error);
    public static ApplicationResult<T> ValidationError(string error) => new(ApplicationResultType.ValidationError, error: error);
    public static ApplicationResult<T> Conflict(string error) => new(ApplicationResultType.Conflict, error: error);
    public static ApplicationResult<T> Failure(string error) 
        => new(ApplicationResultType.Error, error: error);
}
