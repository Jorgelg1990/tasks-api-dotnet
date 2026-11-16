using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TasksApi.Application.Auth;
using TasksApi.Application.Common;

namespace TasksApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var result = await auth.RegisterAsync(request, cancellationToken);
        if (result.Kind != ResultKind.Success)
        {
            if (result.Message?.StartsWith("conflict:", StringComparison.Ordinal) == true)
                return Conflict(new ProblemDetails { Title = "Conflicto", Detail = result.Message["conflict:".Length..], Status = StatusCodes.Status409Conflict });
            return BadRequest(new ProblemDetails { Title = "Solicitud inválida", Detail = result.Message, Status = StatusCodes.Status400BadRequest });
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await auth.LoginAsync(request, cancellationToken);
        if (result.Kind != ResultKind.Success)
            return Unauthorized(new ProblemDetails { Title = "No autorizado", Detail = "Credenciales inválidas.", Status = StatusCodes.Status401Unauthorized });

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshRequestDto request, CancellationToken cancellationToken)
    {
        var result = await auth.RefreshAsync(request.RefreshToken, cancellationToken);
        if (result.Kind != ResultKind.Success)
            return Unauthorized(new ProblemDetails { Title = "No autorizado", Detail = "Token de actualización inválido.", Status = StatusCodes.Status401Unauthorized });

        return Ok(result.Value);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto request, CancellationToken cancellationToken)
    {
        var result = await auth.LogoutAsync(request.RefreshToken, cancellationToken);
        if (result.Kind != ResultKind.Success)
            return Unauthorized(new ProblemDetails { Title = "No autorizado", Detail = "Token de actualización inválido.", Status = StatusCodes.Status401Unauthorized });

        return NoContent();
    }
}
