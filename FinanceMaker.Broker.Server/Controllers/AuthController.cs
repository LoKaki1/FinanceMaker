using FinanceMaker.Broker.Services;
using FinanceMaker.Broker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanceMaker.Broker.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService m_AuthService;

    public AuthController(IAuthService authService)
    {
        m_AuthService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await m_AuthService.RegisterAsync(request.Username, request.Email, request.Password);
        if (!response.Success)
        {
            return BadRequest(new { error = response.Error });
        }
        return Ok(new { token = response.Token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await m_AuthService.LoginAsync(request.Username, request.Password);
        if (!response.Success)
        {
            return Unauthorized(new { error = response.Error });
        }
        return Ok(new { token = response.Token });
    }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
