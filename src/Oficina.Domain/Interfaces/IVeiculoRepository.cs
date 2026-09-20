using Oficina.Domain.Entities;

namespace Oficina.Domain.Interfaces;

public interface IVeiculoRepository
{
    Task<IEnumerable<Veiculo>> ObterTodosAsync();
    Task<Veiculo?> ObterPorIdAsync(int id);
    Task AdicionarAsync(Veiculo cliente);
    Task AtualizarAsync(Veiculo cliente);
    Task RemoverAsync(int id);
}
