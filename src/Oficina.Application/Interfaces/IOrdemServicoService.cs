using Oficina.Application.DTOs.OrdemServico;

namespace Oficina.Application.Interfaces;

public interface IOrdemServicoService
{
    Task<IEnumerable<OrdemServicoDto>> ObterTodosAsync();

    Task<OrdemServicoDto?> ObterPorIdAsync(int id);

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

    Task<OrdemServicoDto> ConcluirDiagnosticoAsync(
        int ordemServicoId);

    Task<OrdemServicoDto> GerarOrcamentoAsync(
        int ordemServicoId);

    Task<OrdemServicoDto> DisponibilizarOrcamentoAsync(
        int ordemServicoId);

    Task<OrdemServicoDto> AprovarOrcamentoAsync(
        int ordemServicoId);

    Task<OrdemServicoDto> RejeitarOrcamentoAsync(
        int ordemServicoId);

    Task<OrdemServicoDto> IniciarExecucaoAsync(
        int ordemServicoId);

    Task<OrdemServicoDto> FinalizarAsync(
        int ordemServicoId);

    Task<OrdemServicoDto> EntregarAsync(
        int ordemServicoId);
}