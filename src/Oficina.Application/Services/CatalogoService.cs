using AutoMapper;
using Oficina.Application.DTOs.Catalogo;
using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;

namespace Oficina.Application.Services;

public class CatalogoService : ICatalogoService
{
    private readonly ICatalogoRepository _repository;
    private readonly IMapper _mapper;

    public CatalogoService(ICatalogoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    #region Basic CRUD
    public async Task<IEnumerable<CatalogoDto>> ObterTodosAsync()
    {
        var catalogos = await _repository.ObterTodosAsync();

        return _mapper.Map<IEnumerable<CatalogoDto>>(catalogos);
    }

    public async Task<CatalogoDto?> ObterPorIdAsync(int id)
    {
        var catalogo = await _repository.ObterPorIdAsync(id);

        return _mapper.Map<CatalogoDto>(catalogo);
    }

    public async Task<CatalogoDto> CriarAsync(CatalogoInputDto dto)
    {
        var catalogo = _mapper.Map<Catalogo>(dto);

        await _repository.AdicionarAsync(catalogo);

        return _mapper.Map<CatalogoDto>(catalogo);
    }

    public async Task<bool> AtualizarAsync(int id, CatalogoInputDto dto)
    {
        var catalogo = await _repository.ObterPorIdAsync(id);

        if (catalogo is null)
            return false;

        catalogo.Atualizar(dto.Nome, dto.Preco, dto.Tipo, dto.Ativo, dto.Descricao);

        await _repository.AtualizarAsync(catalogo);

        return true;
    }

    public async Task RemoverAsync(int id) => await _repository.RemoverAsync(id);
    #endregion
}
