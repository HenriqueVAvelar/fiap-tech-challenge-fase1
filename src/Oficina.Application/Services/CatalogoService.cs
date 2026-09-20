using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;

namespace Oficina.Application.Services;

public class CatalogoService : ICatalogoService
{
    private readonly ICatalogoRepository _repository;

    public CatalogoService(ICatalogoRepository repository)
    {
        _repository = repository;
    }

    #region Basic CRUD
    public async Task<IEnumerable<Catalogo>> ObterTodosAsync() 
        => await _repository.ObterTodosAsync();

    public async Task<Catalogo?> ObterPorIdAsync(int id) 
        => await _repository.ObterPorIdAsync(id);

    public async Task CriarAsync(Catalogo catalogo) 
        => await _repository.AdicionarAsync(catalogo);

    public async Task AtualizarAsync(Catalogo catalogo) 
        => await _repository.AtualizarAsync(catalogo);

    public async Task RemoverAsync(int id) 
        => await _repository.RemoverAsync(id);
    #endregion
}
