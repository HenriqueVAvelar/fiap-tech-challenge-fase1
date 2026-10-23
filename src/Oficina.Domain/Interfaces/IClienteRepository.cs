using Oficina.Domain.Entities;

namespace Oficina.Domain.Interfaces;

public interface IClienteRepository
{
    Task<IEnumerable<Cliente>> ObterTodosAsync();
    Task<Cliente?> ObterPorIdAsync(int id);
    Task<Cliente?> ObterPorDocumentoAsync(string numero);
    Task<bool> DocumentoExisteAsync(string numero, int? ignorarId = null);
    Task AdicionarAsync(Cliente cliente);
    Task AtualizarAsync(Cliente cliente);
    Task RemoverAsync(int id);
}
