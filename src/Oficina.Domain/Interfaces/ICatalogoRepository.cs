using Oficina.Domain.Entities;

namespace Oficina.Domain.Interfaces;

public interface ICatalogoRepository
{
    Task<IEnumerable<Catalogo>> ObterTodosAsync();
    Task<Catalogo?> ObterPorIdAsync(int id);
    Task AdicionarAsync(Catalogo catalogo);
    Task AtualizarAsync(Catalogo catalogo);
    Task RemoverAsync(int id);
}
