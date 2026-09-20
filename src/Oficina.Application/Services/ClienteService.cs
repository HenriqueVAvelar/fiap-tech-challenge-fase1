using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;

namespace Oficina.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository)
    {
        _repository = repository;
    }

    #region Basic CRUD
    public async Task<IEnumerable<Cliente>> ObterTodosAsync()
        => await _repository.ObterTodosAsync();

    public async Task<Cliente?> ObterPorIdAsync(int id)
        => await _repository.ObterPorIdAsync(id);

    public async Task CriarAsync(Cliente cliente)
        => await _repository.AdicionarAsync(cliente);

    public async Task AtualizarAsync(Cliente cliente)
        => await _repository.AtualizarAsync(cliente);

    public async Task RemoverAsync(int id)
        => await _repository.RemoverAsync(id);
    #endregion
}
