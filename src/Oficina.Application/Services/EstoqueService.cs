using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;

namespace Oficina.Application.Services;

public class EstoqueService : IEstoqueService
{
    private readonly IEstoqueRepository _repository;

    public EstoqueService(IEstoqueRepository repository)
    {
        _repository = repository;
    }

    #region Basic CRUD
    public async Task<IEnumerable<Estoque>> ObterTodosAsync()
        => await _repository.ObterTodosAsync();

    public async Task<Estoque?> ObterPorIdAsync(int id)
        => await _repository.ObterPorIdAsync(id);

    public async Task CriarAsync(Estoque estoque)
        => await _repository.AdicionarAsync(estoque);

    public async Task AtualizarAsync(Estoque estoque)
        => await _repository.AtualizarAsync(estoque);

    public async Task RemoverAsync(int id)
        => await _repository.RemoverAsync(id);
    #endregion
}
