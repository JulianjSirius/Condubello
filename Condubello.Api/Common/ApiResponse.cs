using Condubello.Application.Common.Results;

namespace Condubello.Api.Common;

/// <summary>Envoltorio estándar de todas las respuestas de la API.</summary>
public record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data,
    IReadOnlyList<ApiError> Errors,
    IReadOnlyList<string> Warnings)
{
    public static ApiResponse<T> Ok(T? data, string message, IReadOnlyList<string>? warnings = null) =>
        new(true, message, data, [], warnings ?? []);

    public static ApiResponse<T> Fail(string message, IReadOnlyList<ApiError>? errors = null, IReadOnlyList<string>? warnings = null) =>
        new(false, message, default, errors ?? [], warnings ?? []);
}
