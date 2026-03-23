namespace Practica_Examen.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Practica_Examen.API.DTOs;
using Practica_Examen.API.Services;
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var (success, message) = await _authService.Register(req);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var (success, message, data) = await _authService.Login(req);
        if (!success) return Unauthorized(new { message });
        return Ok(data);
    }