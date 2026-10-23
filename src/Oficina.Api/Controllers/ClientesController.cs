using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces;
using Oficina.Domain.Enums;

namespace Oficina.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ClientesController : ApiControllerBase
{
    private readonly IClienteService _service;

    public ClientesController(IClienteService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.ObterTodosAsync());

    [HttpGet("{documento}")]
    public Task<IActionResult> GetByDocumento(string documento)
        => ExecutarAsync(async () =>
        {
            var result = await _service.ObterPorDocumentoAsync(documento);

            return result is null ? NotFound() : Ok(result);
        });

    [HttpPost]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public Task<IActionResult> Create([FromBody] ClienteInputDto cliente)
        => ExecutarAsync(async () =>
        {
            var criado = await _service.CriarAsync(cliente);

            return CreatedAtAction(nameof(GetByDocumento), new { documento = criado.Documento }, criado);
        });

    [HttpPut("{documento}")]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public Task<IActionResult> Update(string documento, [FromBody] ClienteInputDto cliente)
        => ExecutarAsync(async () =>
            await _service.AtualizarAsync(documento, cliente) ? NoContent() : NotFound());

    [HttpDelete("{documento}")]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public Task<IActionResult> Delete(string documento)
        => ExecutarAsync(async () =>
            await _service.RemoverAsync(documento) ? NoContent() : NotFound());
}
