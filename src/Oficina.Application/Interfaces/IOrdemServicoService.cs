using Oficina.Application.DTOs.OrdemServico;

namespace Oficina.Application.Interfaces;

public interface IOrdemServicoService
{
    Task<IEnumerable<OrdemServicoDto>> ObterTodosAsync();

    /// <summary>Ordens de Serviço do cliente identificado pelo CPF/CNPJ (com ou sem máscara).</summary>
    Task<IEnumerable<OrdemServicoDto>> ObterPorDocumentoAsync(string documento);

    Task<OrdemServicoDto> CriarAsync(
        CriarOrdemServicoInputDto input);

    Task<OrdemServicoDto> AdicionarServicoAsync(
        int ordemServicoId,
        AdicionarServicoOrdemInputDto input);

    Task<OrdemServicoDto> IniciarDiagnosticoAsync(
        int ordemServicoId);

    Task<OrdemServicoDto> AdicionarPecaInsumoAsync(
        int ordemServicoId,
        AdicionarPecaInsumoOrdemInputDto input);

    Task<OrdemServicoDto> RegistrarConsumoAsync(
        int ordemServicoId,
        RegistrarConsumoOrdemInputDto input);

    Task<OrdemServicoDto> ConcluirDiagnosticoAsync(
        int ordemServicoId);

    Task<OrdemServicoDto> GerarOrcamentoAsync(
        int ordemServicoId);

    Task<OrdemServicoDto> DisponibilizarOrcamentoAsync(
        int ordemServicoId);

    /// <summary>
    /// Aprova o orçamento da OS do cliente (CPF/CNPJ) que está aguardando decisão.
    /// A placa só é necessária quando o cliente tem mais de uma OS aguardando.
    /// </summary>
    Task<OrdemServicoDto> AprovarOrcamentoAsync(
        string documento,
        string? placa = null);

    /// <summary>
    /// Rejeita o orçamento da OS do cliente (CPF/CNPJ) que está aguardando decisão.
    /// A placa só é necessária quando o cliente tem mais de uma OS aguardando.
    /// </summary>
    Task<OrdemServicoDto> RejeitarOrcamentoAsync(
        string documento,
        string? placa = null);

    Task<OrdemServicoDto> IniciarExecucaoAsync(
        int ordemServicoId);

    Task<OrdemServicoDto> FinalizarAsync(
        int ordemServicoId);

    Task<OrdemServicoDto> EntregarAsync(
        int ordemServicoId);
}