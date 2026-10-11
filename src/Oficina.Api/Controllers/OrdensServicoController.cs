using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs.OrdemServico;
using Oficina.Application.Interfaces;
using Oficina.Domain.Enums;

namespace Oficina.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdensServicoController : ApiControllerBase
{
    private const string Administrador = nameof(EnumsUsuario.PerfilUsuario.Administrador);

    private readonly IOrdemServicoService _service;

    public OrdensServicoController(IOrdemServicoService service)
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
    [Authorize(Roles = Administrador)]
    public Task<IActionResult> Create([FromBody] CriarOrdemServicoInputDto ordem)
        => ExecutarAsync(async () =>
        {
            var criada = await _service.CriarAsync(ordem);

            return CreatedAtAction(nameof(GetById), new { id = criada.Id }, criada);
        });

    [HttpPost("{id:int}/servicos")]
    [Authorize(Roles = Administrador)]
    public Task<IActionResult> AdicionarServico(int id, [FromBody] AdicionarServicoOrdemInputDto servico)
        => ExecutarAsync(async () => Ok(await _service.AdicionarServicoAsync(id, servico)));

    [HttpPost("{id:int}/pecas-insumos")]
    [Authorize(Roles = Administrador)]
    public Task<IActionResult> AdicionarPecaInsumo(int id, [FromBody] AdicionarPecaInsumoOrdemInputDto item)
        => ExecutarAsync(async () => Ok(await _service.AdicionarPecaInsumoAsync(id, item)));

    [HttpPost("{id:int}/iniciar-diagnostico")]
    [Authorize(Roles = Administrador)]
    public Task<IActionResult> IniciarDiagnostico(int id)
        => ExecutarAsync(async () => Ok(await _service.IniciarDiagnosticoAsync(id)));

    [HttpPost("{id:int}/concluir-diagnostico")]
    [Authorize(Roles = Administrador)]
    public Task<IActionResult> ConcluirDiagnostico(int id)
        => ExecutarAsync(async () => Ok(await _service.ConcluirDiagnosticoAsync(id)));

    [HttpPost("{id:int}/gerar-orcamento")]
    [Authorize(Roles = Administrador)]
    public Task<IActionResult> GerarOrcamento(int id)
        => ExecutarAsync(async () => Ok(await _service.GerarOrcamentoAsync(id)));

    [HttpPost("{id:int}/disponibilizar-orcamento")]
    [Authorize(Roles = Administrador)]
    public Task<IActionResult> DisponibilizarOrcamento(int id)
        => ExecutarAsync(async () => Ok(await _service.DisponibilizarOrcamentoAsync(id)));

    [HttpPost("{id:int}/aprovar-orcamento")]
    [Authorize(Roles = Administrador)]
    public Task<IActionResult> AprovarOrcamento(int id)
        => ExecutarAsync(async () => Ok(await _service.AprovarOrcamentoAsync(id)));

    [HttpPost("{id:int}/rejeitar-orcamento")]
    [Authorize(Roles = Administrador)]
    public Task<IActionResult> RejeitarOrcamento(int id)
        => ExecutarAsync(async () => Ok(await _service.RejeitarOrcamentoAsync(id)));

    [HttpPost("{id:int}/iniciar-execucao")]
    [Authorize(Roles = Administrador)]
    public Task<IActionResult> IniciarExecucao(int id)
        => ExecutarAsync(async () => Ok(await _service.IniciarExecucaoAsync(id)));

    [HttpPost("{id:int}/finalizar")]
    [Authorize(Roles = Administrador)]
    public Task<IActionResult> Finalizar(int id)
        => ExecutarAsync(async () => Ok(await _service.FinalizarAsync(id)));

    [HttpPost("{id:int}/entregar")]
    [Authorize(Roles = Administrador)]
    public Task<IActionResult> Entregar(int id)
        => ExecutarAsync(async () => Ok(await _service.EntregarAsync(id)));
}
