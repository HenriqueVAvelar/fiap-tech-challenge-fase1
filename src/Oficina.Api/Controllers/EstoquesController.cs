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
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador) + "," + nameof(EnumsUsuario.PerfilUsuario.Mecanico))]
    public Task<IActionResult> Update(int id, [FromBody] EstoqueUpdateInputDto estoque)
        => ExecutarAsync(async () =>
            await _service.AtualizarAsync(id, estoque) ? NoContent() : NotFound());

    [HttpPost("{id:int}/entrada")]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public Task<IActionResult> RegistrarEntrada(int id, [FromBody] EstoqueMovimentacaoInputDto movimentacao)
        => ExecutarAsync(async () =>
        {
            var estoque = await _service.RegistrarEntradaAsync(id, movimentacao.Quantidade);

            return estoque is null ? NotFound() : Ok(estoque);
        });

    [HttpPost("{id:int}/baixa")]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador) + "," + nameof(EnumsUsuario.PerfilUsuario.Mecanico))]
    public Task<IActionResult> RegistrarBaixa(int id, [FromBody] EstoqueMovimentacaoInputDto movimentacao)
        =>  ExecutarAsync(async () =>
        {
            var estoque = await _service.RegistrarBaixaAsync(id, movimentacao.Quantidade);

            return estoque is null ? NotFound() : Ok(estoque);
        });

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(EnumsUsuario.PerfilUsuario.Administrador))]
    public Task<IActionResult> Delete(int id)
        => ExecutarAsync(async () =>
            await _service.RemoverAsync(id) ? NoContent() : NotFound());
}
