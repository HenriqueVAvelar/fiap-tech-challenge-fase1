using Oficina.Domain.Entities;

namespace Oficina.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<IEnumerable<Usuario>> ObterTodosAsync();

    Task<Usuario?> ObterPorIdAsync(int id);

    Task<Usuario?> ObterPorUserNameAsync(string userName);

    Task<bool> UserNameExisteAsync(string userName, int? ignorarId = null);

    Task AdicionarAsync(Usuario usuario);

    Task AtualizarAsync(Usuario usuario);

    Task RemoverAsync(int id);
}
