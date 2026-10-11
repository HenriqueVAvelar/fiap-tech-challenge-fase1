using AutoMapper;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;

namespace Oficina.Application.Services;

public class EstoqueService : IEstoqueService
{
    private readonly IEstoqueRepository _repository;
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly IMapper _mapper;

    public EstoqueService(IEstoqueRepository repository, ICatalogoRepository catalogoRepository, IMapper mapper)
    {
        _repository = repository;
        _catalogoRepository = catalogoRepository;
        _mapper = mapper;
    }

    #region Basic CRUD
    public async Task<IEnumerable<EstoqueDto>> ObterTodosAsync()
    {
        var estoques = await _repository.ObterTodosAsync();

        return _mapper.Map<IEnumerable<EstoqueDto>>(estoques);
    }

    public async Task<EstoqueDto?> ObterPorIdAsync(int id)
    {
        var estoque = await _repository.ObterPorIdAsync(id);

        return estoque is null ? null : _mapper.Map<EstoqueDto>(estoque);
    }

    public async Task<EstoqueDto> CriarAsync(EstoqueInputDto dto)
    {
        if (await _catalogoRepository.ObterPorIdAsync(dto.CatalogoId) is null)
            throw new KeyNotFoundException($"Item de catálogo {dto.CatalogoId} não encontrado.");

        var estoque = new Estoque(dto.Quantidade)
        {
            CatalogoId = dto.CatalogoId
        };

        await _repository.AdicionarAsync(estoque);

        return _mapper.Map<EstoqueDto>(estoque);
    }

    public async Task<bool> AtualizarAsync(int id, EstoqueUpdateInputDto dto)
    {
        var estoque = await _repository.ObterPorIdAsync(id);

        if (estoque is null)
            return false;

        if (dto.Quantidade < 0)
            throw new ArgumentOutOfRangeException(nameof(dto.Quantidade), "A quantidade não pode ser negativa.");

        estoque.Quantidade = dto.Quantidade;

        await _repository.AtualizarAsync(estoque);

        return true;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var estoque = await _repository.ObterPorIdAsync(id);

        if (estoque is null)
            return false;

        await _repository.RemoverAsync(id);

        return true;
    }
    #endregion

    public async Task<EstoqueDto?> RegistrarEntradaAsync(int id, int quantidade)
    {
        var estoque = await _repository.ObterPorIdAsync(id);

        if (estoque is null)
            return null;

        estoque.RegistrarEntrada(quantidade);

        await _repository.AtualizarAsync(estoque);

        return _mapper.Map<EstoqueDto>(estoque);
    }

    public async Task<EstoqueDto?> RegistrarBaixaAsync(int id, int quantidade)
    {
        var estoque = await _repository.ObterPorIdAsync(id);

        if (estoque is null)
            return null;

        estoque.Consumir(quantidade);

        await _repository.AtualizarAsync(estoque);

        return _mapper.Map<EstoqueDto>(estoque);
    }
}
