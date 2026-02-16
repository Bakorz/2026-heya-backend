using CampusRooms.Api.DTOs.Auth;
using CampusRooms.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusRooms.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await authService.LoginAsync(request);
        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequestDto request)
    {
        var result = await authService.RegisterAsync(request);
        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }
}
