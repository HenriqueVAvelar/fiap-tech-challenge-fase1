using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces;

public interface IVeiculoService
{
    Task<IEnumerable<VeiculoDto>> ObterTodosAsync();

    Task<VeiculoDto?> ObterPorIdAsync(int id);

    Task<VeiculoDto> CriarAsync(VeiculoInputDto veiculo);

    Task<bool> AtualizarAsync(int id, VeiculoInputDto veiculo);

    Task<bool> RemoverAsync(int id);
}
