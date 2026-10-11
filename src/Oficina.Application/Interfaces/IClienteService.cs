using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<ClienteDto>> ObterTodosAsync();

    Task<ClienteDto?> ObterPorIdAsync(int id);

    Task<ClienteDto> CriarAsync(ClienteInputDto cliente);

    Task<bool> AtualizarAsync(int id, ClienteInputDto cliente);

    Task<bool> RemoverAsync(int id);
}
