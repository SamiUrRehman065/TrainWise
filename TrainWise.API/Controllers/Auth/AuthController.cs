using Microsoft.AspNetCore.Mvc;
using TrainWise.API.Contracts.Auth;
using TrainWise.API.Services.Auth;

namespace TrainWise.API.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (result is null)
        {
            return Unauthorized();
        }

        return Ok(result);
    }

    [HttpPost("signup")]
    public async Task<ActionResult<LoginResponse>> SignupAsync(
        [FromBody] SignupRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.SignupAsync(request, cancellationToken);
        if (result is null)
        {
            return Conflict(new { error = "Username already exists or invalid credentials." });
        }

        return Ok(result);
    }
}
