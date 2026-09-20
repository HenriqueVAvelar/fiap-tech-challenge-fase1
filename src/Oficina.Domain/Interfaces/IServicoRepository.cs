using Oficina.Domain.Entities;

namespace Oficina.Domain.Interfaces;

public interface IServicoRepository
{
    Task<IEnumerable<Servico>> ObterTodosAsync();
    Task<Servico?> ObterPorIdAsync(int id);
    Task AdicionarAsync(Servico servico);
    Task AtualizarAsync(Servico servico);
    Task RemoverAsync(int id);
}
