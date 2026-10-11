using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs.Catalogo;
using Oficina.Application.Interfaces;
using Oficina.Domain.Enums;

namespace Oficina.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class CatalogoController : ControllerBase
{
    private readonly ICatalogoService _service;

    public CatalogoController(ICatalogoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.ObterTodosAsync();
        return Ok(result);
    }

    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.ObterPorIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public async Task<IActionResult> Create([FromBody] CatalogoInputDto catalogo)
    {
        var criado = await _service.CriarAsync(catalogo);

        return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public async Task<IActionResult> Update(int id, [FromBody] CatalogoInputDto catalogo)
    {
        var atualizado = await _service.AtualizarAsync(id, catalogo);

        if (!atualizado)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.RemoverAsync(id);
        return NoContent();
    }
}
