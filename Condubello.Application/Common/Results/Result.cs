namespace Condubello.Application.Common.Results;

public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}

/// <summary>Error asociado a un campo concreto de la petición.</summary>
public record ApiError(string Campo, string Mensaje);

/// <summary>Permite crear un fallo sin conocer el tipo concreto (usado por los behaviors de MediatR).</summary>
public interface IFailureResult<out TSelf>
{
    static abstract TSelf Forbidden(string mensaje);
}

/// <summary>
/// Resultado de un caso de uso: éxito (con advertencias opcionales) o fallo con su tipo y errores.
/// </summary>
public class Result<T> : IFailureResult<Result<T>>
{
    private Result(
        bool isSuccess,
        T? value,
        ErrorType errorType,
        string message,
        IReadOnlyList<ApiError> errors,
        IReadOnlyList<string> warnings)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorType = errorType;
        Message = message;
        Errors = errors;
        Warnings = warnings;
    }

    public bool IsSuccess { get; }
    public T? Value { get; }
    public ErrorType ErrorType { get; }
    public string Message { get; }
    public IReadOnlyList<ApiError> Errors { get; }
    public IReadOnlyList<string> Warnings { get; }

    public static Result<T> Success(T value, IEnumerable<string>? warnings = null) =>
        new(true, value, ErrorType.None, string.Empty, [], warnings?.ToList() ?? []);

    public static Result<T> Invalid(string campo, string mensaje) =>
        new(false, default, ErrorType.Validation, mensaje, [new ApiError(campo, mensaje)], []);

    public static Result<T> Validation(IEnumerable<ApiError> errors)
    {
        var lista = errors.ToList();
        var mensaje = lista.Count == 1 ? lista[0].Mensaje : "Los datos enviados no son válidos.";
        return new(false, default, ErrorType.Validation, mensaje, lista, []);
    }

    public static Result<T> NotFound(string mensaje) =>
        new(false, default, ErrorType.NotFound, mensaje, [], []);

    public static Result<T> Conflict(string campo, string mensaje) =>
        new(false, default, ErrorType.Conflict, mensaje, [new ApiError(campo, mensaje)], []);

    public static Result<T> Conflict(IEnumerable<ApiError> errors)
    {
        var lista = errors.ToList();
        var mensaje = lista.Count == 1 ? lista[0].Mensaje : "Ya existe un registro con los mismos datos.";
        return new(false, default, ErrorType.Conflict, mensaje, lista, []);
    }

    public static Result<T> Unauthorized(string mensaje) =>
        new(false, default, ErrorType.Unauthorized, mensaje, [], []);

    public static Result<T> Forbidden(string mensaje) =>
        new(false, default, ErrorType.Forbidden, mensaje, [], []);
}
