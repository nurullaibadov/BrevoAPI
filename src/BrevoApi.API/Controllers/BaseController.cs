using BrevoApi.Application.Common;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BrevoApi.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && int.TryParse(claim, out var id) ? id : 0;
    }

    protected string GetCurrentUserEmail()
        => User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    protected IActionResult OkResult<T>(T data, string? message = null)
        => Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult FailResult(string message, int statusCode = 400)
        => StatusCode(statusCode, ApiResponse.Fail(message));
}
