using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs.Auth;
using Oficina.Application.Interfaces;

namespace Oficina.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginInputDto login)
    {
        var result = await _service.LoginAsync(login);

        if (result is null)
            return Unauthorized("UserName ou senha inválidos.");

        return Ok(result);
    }
}
