using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces;

public interface IServicoService
{
    Task<IEnumerable<ServicoDto>> ObterTodosAsync();

    Task<ServicoDto?> ObterPorIdAsync(int id);

    Task<ServicoDto> CriarAsync(ServicoInputDto servico);

    Task<bool> AtualizarAsync(int id, ServicoInputDto servico);

    Task<bool> RemoverAsync(int id);
}
