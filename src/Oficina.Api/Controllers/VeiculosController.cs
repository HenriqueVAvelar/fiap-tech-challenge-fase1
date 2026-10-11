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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.ObterPorIdAsync(id);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public Task<IActionResult> Create([FromBody] VeiculoInputDto veiculo)
        => ExecutarAsync(async () =>
        {
            var criado = await _service.CriarAsync(veiculo);

            return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
        });

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public Task<IActionResult> Update(int id, [FromBody] VeiculoInputDto veiculo)
        => ExecutarAsync(async () =>
            await _service.AtualizarAsync(id, veiculo) ? NoContent() : NotFound());

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public Task<IActionResult> Delete(int id)
        => ExecutarAsync(async () =>
            await _service.RemoverAsync(id) ? NoContent() : NotFound());
}
