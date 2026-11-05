using Condubello.Api.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Condubello.Api.Middleware;

/// <summary>Convierte las excepciones no controladas en respuestas con el envoltorio estándar.</summary>
public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            await EscribirAsync(context, StatusCodes.Status409Conflict, "Ya existe un registro con los mismos datos únicos.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        {
            await EscribirAsync(context, StatusCodes.Status409Conflict,
                "La operación no se puede completar porque existen registros relacionados.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado en {Method} {Path}", context.Request.Method, context.Request.Path);
            await EscribirAsync(context, StatusCodes.Status500InternalServerError,
                "Ocurrió un error inesperado. Intente nuevamente.");
        }
    }

    private static Task EscribirAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(message));
    }
}
