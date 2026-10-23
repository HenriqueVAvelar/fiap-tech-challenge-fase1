using Oficina.Domain.Entities;

namespace Oficina.Domain.Interfaces;

public interface IVeiculoRepository
{
    Task<IEnumerable<Veiculo>> ObterTodosAsync();
    Task<Veiculo?> ObterPorIdAsync(int id);
    Task<Veiculo?> ObterPorPlacaAsync(string placa);
    Task<bool> PlacaExisteAsync(string placa, int? ignorarId = null);
    Task AdicionarAsync(Veiculo cliente);
    Task AtualizarAsync(Veiculo cliente);
    Task RemoverAsync(int id);
}
