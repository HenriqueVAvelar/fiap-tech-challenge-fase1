using AutoMapper;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;

namespace Oficina.Application.Services;

public class ServicoService : IServicoService
{
    private readonly IServicoRepository _repository;
    private readonly IMapper _mapper;

    public ServicoService(IServicoRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    #region Basic CRUD
    public async Task<IEnumerable<ServicoDto>> ObterTodosAsync()
    {
        var servicos = await _repository.ObterTodosAsync();

        return _mapper.Map<IEnumerable<ServicoDto>>(servicos);
    }

    public async Task<ServicoDto?> ObterPorIdAsync(int id)
    {
        var servico = await _repository.ObterPorIdAsync(id);

        return servico is null ? null : _mapper.Map<ServicoDto>(servico);
    }

    public async Task<ServicoDto> CriarAsync(ServicoInputDto dto)
    {
        Validar(dto);

        var servico = new Servico
        {
            Descricao = dto.Descricao.Trim(),
            Valor = dto.Valor,
            Ativo = dto.Ativo
        };

        await _repository.AdicionarAsync(servico);

        return _mapper.Map<ServicoDto>(servico);
    }

    public async Task<bool> AtualizarAsync(int id, ServicoInputDto dto)
    {
        var servico = await _repository.ObterPorIdAsync(id);

        if (servico is null)
            return false;

        Validar(dto);

        servico.Descricao = dto.Descricao.Trim();
        servico.Valor = dto.Valor;
        servico.Ativo = dto.Ativo;

        await _repository.AtualizarAsync(servico);

        return true;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var servico = await _repository.ObterPorIdAsync(id);

        if (servico is null)
            return false;

        await _repository.RemoverAsync(id);

        return true;
    }
    #endregion

    private static void Validar(ServicoInputDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao))
            throw new ArgumentException("A descrição do serviço é obrigatória.", nameof(dto.Descricao));

        if (dto.Valor < 0)
            throw new ArgumentException("O valor do serviço não pode ser negativo.", nameof(dto.Valor));
    }
}
