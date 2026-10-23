using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces;

public interface IVeiculoService
{
    Task<IEnumerable<VeiculoDto>> ObterTodosAsync();

    Task<VeiculoDto?> ObterPorPlacaAsync(string placa);

    Task<VeiculoDto> CriarAsync(VeiculoInputDto veiculo);

    Task<bool> AtualizarAsync(string placa, VeiculoInputDto veiculo);

    Task<bool> RemoverAsync(string placa);
}
