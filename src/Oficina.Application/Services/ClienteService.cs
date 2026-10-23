using AutoMapper;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;
using Oficina.Domain.ValueObjects;

namespace Oficina.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;
    private readonly IMapper _mapper;

    public ClienteService(IClienteRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    #region Basic CRUD
    public async Task<IEnumerable<ClienteDto>> ObterTodosAsync()
    {
        var clientes = await _repository.ObterTodosAsync();

        return _mapper.Map<IEnumerable<ClienteDto>>(clientes);
    }

    public async Task<ClienteDto?> ObterPorDocumentoAsync(string documento)
    {
        var cliente = await _repository.ObterPorDocumentoAsync(new Documento(documento).Numero);

        return cliente is null ? null : _mapper.Map<ClienteDto>(cliente);
    }

    public async Task<ClienteDto> CriarAsync(ClienteInputDto dto)
    {
        var documento = new Documento(dto.Documento);
        await GarantirDocumentoDisponivelAsync(documento);

        var cliente = new Cliente(dto.Nome, documento);

        await _repository.AdicionarAsync(cliente);

        return _mapper.Map<ClienteDto>(cliente);
    }

    public async Task<bool> AtualizarAsync(string documento, ClienteInputDto dto)
    {
        var cliente = await _repository.ObterPorDocumentoAsync(new Documento(documento).Numero);

        if (cliente is null)
            return false;

        var novoDocumento = new Documento(dto.Documento);
        await GarantirDocumentoDisponivelAsync(novoDocumento, cliente.Id);

        cliente.Atualizar(dto.Nome, novoDocumento);

        await _repository.AtualizarAsync(cliente);

        return true;
    }

    public async Task<bool> RemoverAsync(string documento)
    {
        var cliente = await _repository.ObterPorDocumentoAsync(new Documento(documento).Numero);

        if (cliente is null)
            return false;

        await _repository.RemoverAsync(cliente.Id);

        return true;
    }
    #endregion

    private async Task GarantirDocumentoDisponivelAsync(Documento documento, int? ignorarId = null)
    {
        if (await _repository.DocumentoExisteAsync(documento.Numero, ignorarId))
            throw new InvalidOperationException($"Já existe um cliente com o documento '{documento.Formatado()}'.");
    }
}
