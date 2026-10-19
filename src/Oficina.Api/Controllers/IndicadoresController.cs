using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.Interfaces;
using Oficina.Domain.Enums;

namespace Oficina.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
public class IndicadoresController : ControllerBase
{
    private readonly ITempoMedioExecucaoService _service;

    public IndicadoresController(
        ITempoMedioExecucaoService service)
    {
        _service = service;
    }

    [HttpGet("tempo-medio-execucao")]
    public async Task<IActionResult> ObterTempoMedioExecucao()
    {
        var resultado = await _service.ObterAsync();

        return Ok(resultado);
    }
}