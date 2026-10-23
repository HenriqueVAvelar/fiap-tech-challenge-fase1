using Oficina.Domain.Entities;

namespace Oficina.Domain.Interfaces;

public interface IOrdemServicoRepository
{
    Task<IEnumerable<OrdemServico>> ObterTodosAsync();
    Task<OrdemServico?> ObterPorIdAsync(int id);
    Task<IEnumerable<OrdemServico>> ObterPorClienteIdAsync(int clienteId);
    Task AdicionarAsync(OrdemServico ordemServico);
    Task AtualizarAsync(OrdemServico ordemServico);
    Task RemoverAsync(int id);
}
