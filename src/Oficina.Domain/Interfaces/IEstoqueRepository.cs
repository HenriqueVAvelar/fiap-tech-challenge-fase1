using Oficina.Domain.Entities;

namespace Oficina.Domain.Interfaces;

public interface IEstoqueRepository
{
    Task<IEnumerable<Estoque>> ObterTodosAsync();
    Task<Estoque?> ObterPorIdAsync(int id);
    Task<Estoque?> ObterPorCatalogoIdAsync(int catalogoId);
    Task AdicionarAsync(Estoque estoque);
    Task AtualizarAsync(Estoque estoque);
    Task RemoverAsync(int id);
}
