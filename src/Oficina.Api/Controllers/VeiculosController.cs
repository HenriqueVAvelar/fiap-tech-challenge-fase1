using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces;
using Oficina.Domain.Enums;

namespace Oficina.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class VeiculosController : ApiControllerBase
{
    private readonly IVeiculoService _service;

    public VeiculosController(IVeiculoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.ObterTodosAsync());
        
    [HttpGet("{placa}")]
    public Task<IActionResult> GetByPlaca(string placa)
        => ExecutarAsync(async () =>
        {
            var result = await _service.ObterPorPlacaAsync(placa);

            return result is null ? NotFound() : Ok(result);
        });

    [HttpPost]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public Task<IActionResult> Create([FromBody] VeiculoInputDto veiculo)
        => ExecutarAsync(async () =>
        {
            var criado = await _service.CriarAsync(veiculo);

            return CreatedAtAction(nameof(GetByPlaca), new { placa = criado.Placa }, criado);
        });

    [HttpPut("{placa}")]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public Task<IActionResult> Update(string placa, [FromBody] VeiculoInputDto veiculo)
        => ExecutarAsync(async () =>
            await _service.AtualizarAsync(placa, veiculo) ? NoContent() : NotFound());

    [HttpDelete("{placa}")]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public Task<IActionResult> Delete(string placa)
        => ExecutarAsync(async () =>
            await _service.RemoverAsync(placa) ? NoContent() : NotFound());
}
