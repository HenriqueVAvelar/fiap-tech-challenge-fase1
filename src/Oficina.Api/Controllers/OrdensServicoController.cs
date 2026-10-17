using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs.OrdemServico;
using Oficina.Application.Interfaces;
using Oficina.Domain.Enums;

namespace Oficina.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class OrdensServicoController : ApiControllerBase
{
    private const string Administrador = nameof(EnumsUsuario.PerfilUsuario.Administrador);
    private const string AdministradorOuMecanico = Administrador + "," + nameof(EnumsUsuario.PerfilUsuario.Mecanico);

    private readonly IOrdemServicoService _service;

    public OrdensServicoController(IOrdemServicoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.ObterTodosAsync());

    [HttpGet("{id:int}")]
    [AllowAnonymous] // Sabendo o Id, qualquer pessoa pode consultar (teremos numero de OS ?)
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

    [HttpPost("{id:int}/consumir-pecas-insumos")]
    [Authorize(Roles = AdministradorOuMecanico)]
    public Task<IActionResult> RegistrarConsumo(int id, [FromBody] RegistrarConsumoOrdemInputDto consumo)
        => ExecutarAsync(async () => Ok(await _service.RegistrarConsumoAsync(id, consumo)));

    [HttpPost("{id:int}/iniciar-diagnostico")]
    [Authorize(Roles = AdministradorOuMecanico)]
    public Task<IActionResult> IniciarDiagnostico(int id)
        => ExecutarAsync(async () => Ok(await _service.IniciarDiagnosticoAsync(id)));

    [HttpPost("{id:int}/concluir-diagnostico")]
    [Authorize(Roles = AdministradorOuMecanico)]
    public Task<IActionResult> ConcluirDiagnostico(int id)
        => ExecutarAsync(async () => Ok(await _service.ConcluirDiagnosticoAsync(id)));

    [HttpPost("{id:int}/gerar-orcamento")]
    [Authorize(Roles = AdministradorOuMecanico)]
    public Task<IActionResult> GerarOrcamento(int id)
        => ExecutarAsync(async () => Ok(await _service.GerarOrcamentoAsync(id)));

    [HttpPost("{id:int}/disponibilizar-orcamento")]
    [Authorize(Roles = AdministradorOuMecanico)]
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
    [Authorize(Roles = AdministradorOuMecanico)]
    public Task<IActionResult> Finalizar(int id)
        => ExecutarAsync(async () => Ok(await _service.FinalizarAsync(id)));

    [HttpPost("{id:int}/entregar")]
    [Authorize(Roles = Administrador)]
    public Task<IActionResult> Entregar(int id)
        => ExecutarAsync(async () => Ok(await _service.EntregarAsync(id)));
}
