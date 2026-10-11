using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces;
using Oficina.Domain.Enums;

namespace Oficina.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ServicosController : ApiControllerBase
{
    private readonly IServicoService _service;

    public ServicosController(IServicoService service)
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
    public Task<IActionResult> Create([FromBody] ServicoInputDto servico)
        => ExecutarAsync(async () =>
        {
            var criado = await _service.CriarAsync(servico);

            return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
        });

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public Task<IActionResult> Update(int id, [FromBody] ServicoInputDto servico)
        => ExecutarAsync(async () =>
            await _service.AtualizarAsync(id, servico) ? NoContent() : NotFound());

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public Task<IActionResult> Delete(int id)
        => ExecutarAsync(async () =>
            await _service.RemoverAsync(id) ? NoContent() : NotFound());
}
