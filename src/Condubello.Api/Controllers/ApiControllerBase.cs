using Condubello.Api.Common;
using Condubello.Application.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Condubello.Api.Controllers;

[ApiController]
[Produces("application/json")]
[ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ApiResponse<object>>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ApiResponse<object>>(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ApiResponse<object>>(StatusCodes.Status409Conflict)]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _sender;

    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>Traduce el resultado del caso de uso al código HTTP y al envoltorio estándar.</summary>
    protected IActionResult HandleResult<T>(Result<T> result, string successMessage, int successStatus = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
            return StatusCode(successStatus, ApiResponse<T>.Ok(result.Value, successMessage, result.Warnings));

        var body = ApiResponse<T>.Fail(result.Message, result.Errors, result.Warnings);
        return result.ErrorType switch
        {
            ErrorType.NotFound => NotFound(body),
            ErrorType.Conflict => Conflict(body),
            ErrorType.Unauthorized => Unauthorized(body),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, body),
            _ => BadRequest(body)
        };
    }
}
