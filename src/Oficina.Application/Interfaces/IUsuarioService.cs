using Oficina.Application.DTOs.Usuario;

namespace Oficina.Application.Interfaces;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioDto>> ObterTodosAsync();

    Task<UsuarioDto?> ObterPorIdAsync(int id);

    Task<UsuarioDto> CriarAsync(UsuarioInputDto usuario);

    Task<bool> AtualizarAsync(int id, UsuarioUpdateInputDto usuario);

    Task<bool> RemoverAsync(int id);
}
