using AutoMapper;
using Oficina.Application.DTOs;
using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;
using Oficina.Domain.ValueObjects;

namespace Oficina.Application.Services;

public class VeiculoService : IVeiculoService
{
    private readonly IVeiculoRepository _repository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IMapper _mapper;

    public VeiculoService(IVeiculoRepository repository, IClienteRepository clienteRepository, IMapper mapper)
    {
        _repository = repository;
        _clienteRepository = clienteRepository;
        _mapper = mapper;
    }

    #region Basic CRUD
    public async Task<IEnumerable<VeiculoDto>> ObterTodosAsync()
    {
        var veiculos = await _repository.ObterTodosAsync();

        return _mapper.Map<IEnumerable<VeiculoDto>>(veiculos);
    }

    public async Task<VeiculoDto?> ObterPorIdAsync(int id)
    {
        var veiculo = await _repository.ObterPorIdAsync(id);

        return veiculo is null ? null : _mapper.Map<VeiculoDto>(veiculo);
    }

    public async Task<VeiculoDto> CriarAsync(VeiculoInputDto dto)
    {
        await GarantirClienteExisteAsync(dto.ClienteId);

        var veiculo = new Veiculo(dto.ClienteId, new Placa(dto.Placa), dto.Marca.Trim(), dto.Modelo.Trim(), dto.Ano);

        await _repository.AdicionarAsync(veiculo);

        return _mapper.Map<VeiculoDto>(veiculo);
    }

    public async Task<bool> AtualizarAsync(int id, VeiculoInputDto dto)
    {
        var veiculo = await _repository.ObterPorIdAsync(id);

        if (veiculo is null)
            return false;

        await GarantirClienteExisteAsync(dto.ClienteId);

        veiculo.ClienteId = dto.ClienteId;
        veiculo.Placa = new Placa(dto.Placa);
        veiculo.Marca = dto.Marca.Trim();
        veiculo.Modelo = dto.Modelo.Trim();
        veiculo.Ano = dto.Ano;

        await _repository.AtualizarAsync(veiculo);

        return true;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var veiculo = await _repository.ObterPorIdAsync(id);

        if (veiculo is null)
            return false;

        await _repository.RemoverAsync(id);

        return true;
    }
    #endregion

    private async Task GarantirClienteExisteAsync(int clienteId)
    {
        if (await _clienteRepository.ObterPorIdAsync(clienteId) is null)
            throw new KeyNotFoundException($"Cliente {clienteId} não encontrado.");
    }
}
