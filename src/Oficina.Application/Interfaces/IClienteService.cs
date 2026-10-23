using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<ClienteDto>> ObterTodosAsync();

    Task<ClienteDto?> ObterPorDocumentoAsync(string documento);

    Task<ClienteDto> CriarAsync(ClienteInputDto cliente);

    Task<bool> AtualizarAsync(string documento, ClienteInputDto cliente);

    Task<bool> RemoverAsync(string documento);
}
