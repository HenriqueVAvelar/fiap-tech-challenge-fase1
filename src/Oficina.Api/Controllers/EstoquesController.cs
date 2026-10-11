using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces;
using Oficina.Domain.Enums;

namespace Oficina.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class EstoquesController : ApiControllerBase
{
    private readonly IEstoqueService _service;

    public EstoquesController(IEstoqueService service)
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
    public Task<IActionResult> Create([FromBody] EstoqueInputDto estoque)
        => ExecutarAsync(async () =>
        {
            var criado = await _service.CriarAsync(estoque);

            return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
        });

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Mecanico))]
    public Task<IActionResult> Update(int id, [FromBody] EstoqueUpdateInputDto estoque)
        => ExecutarAsync(async () =>
            await _service.AtualizarAsync(id, estoque) ? NoContent() : NotFound());

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public Task<IActionResult> Delete(int id)
        => ExecutarAsync(async () =>
            await _service.RemoverAsync(id) ? NoContent() : NotFound());
}
