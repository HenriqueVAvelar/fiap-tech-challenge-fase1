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
    private const string AdministradorOuMecanico = Administrador + "," + nameof(EnumsUsuario.PerfilUsuario.Mecanico);

    private readonly IOrdemServicoService _service;

    public OrdensServicoController(IOrdemServicoService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = Administrador)]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.ObterTodosAsync());

    
    [HttpGet("{documento}")]
    [AllowAnonymous]
    public Task<IActionResult> GetByDocumento(string documento)
        => ExecutarAsync(async () => Ok(await _service.ObterPorDocumentoAsync(documento)));

    [HttpPost]
    [Authorize(Roles = Administrador)]
    public Task<IActionResult> Create([FromBody] CriarOrdemServicoInputDto ordem)
        => ExecutarAsync(async () =>
        {
            var criada = await _service.CriarAsync(ordem);

            var documento = new string(ordem.Documento.Where(char.IsDigit).ToArray());

            return CreatedAtAction(nameof(GetByDocumento), new { documento }, criada);
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

    /// <summary>
    /// Aprova o orçamento da OS do cliente que está aguardando decisão, identificado pelo CPF/CNPJ.
    /// Se o cliente tiver mais de uma OS aguardando, informe a placa do veículo.
    /// </summary>
    [HttpPost("{documento}/aprovar-orcamento")]
    public Task<IActionResult> AprovarOrcamento(string documento, [FromQuery] string? placa = null)
        => ExecutarAsync(async () => Ok(await _service.AprovarOrcamentoAsync(documento, placa)));

    /// <summary>
    /// Rejeita o orçamento da OS do cliente que está aguardando decisão, identificado pelo CPF/CNPJ.
    /// Se o cliente tiver mais de uma OS aguardando, informe a placa do veículo.
    /// </summary>
    [HttpPost("{documento}/rejeitar-orcamento")]
    public Task<IActionResult> RejeitarOrcamento(string documento, [FromQuery] string? placa = null)
        => ExecutarAsync(async () => Ok(await _service.RejeitarOrcamentoAsync(documento, placa)));

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
