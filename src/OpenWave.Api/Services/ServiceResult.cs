namespace OpenWave.Api.Services;

/// <summary>
/// Expected business outcomes that are not a success. Infrastructure failures throw instead.
/// </summary>
public enum ServiceError
{
    None,
    Invalid,
    NotFound,
    Conflict,
    Unauthorized,
}

/// <summary>
/// Either a value or a <see cref="ServiceError"/> with a Spanish message for the client.
/// </summary>
public sealed class ServiceResult<T>
{
    private ServiceResult(T? value, ServiceError error, string? message, string? field)
    {
        Value = value;
        Error = error;
        Message = message;
        Field = field;
    }

    public T? Value { get; }

    public ServiceError Error { get; }

    /// <summary>
    /// Spanish message shown to the user; null on success.
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Name of the invalid request field, for <see cref="ServiceError.Invalid"/>.
    /// </summary>
    public string? Field { get; }

    public bool IsSuccess => Error == ServiceError.None;

    public static ServiceResult<T> Success(T value) => new(value, ServiceError.None, null, null);

    public static ServiceResult<T> Failure(ServiceError error, string message, string? field = null) =>
        new(default, error, message, field);
}
