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

    public async Task<VeiculoDto?> ObterPorPlacaAsync(string placa)
    {
        var veiculo = await _repository.ObterPorPlacaAsync(new Placa(placa).Valor);

        return veiculo is null ? null : _mapper.Map<VeiculoDto>(veiculo);
    }

    public async Task<VeiculoDto> CriarAsync(VeiculoInputDto dto)
    {
        await GarantirClienteExisteAsync(dto.ClienteId);

        var placa = new Placa(dto.Placa);
        await GarantirPlacaDisponivelAsync(placa);

        var veiculo = new Veiculo(dto.ClienteId, placa, dto.Marca.Trim(), dto.Modelo.Trim(), dto.Ano);

        await _repository.AdicionarAsync(veiculo);

        return _mapper.Map<VeiculoDto>(veiculo);
    }

    public async Task<bool> AtualizarAsync(string placa, VeiculoInputDto dto)
    {
        var veiculo = await _repository.ObterPorPlacaAsync(new Placa(placa).Valor);

        if (veiculo is null)
            return false;

        await GarantirClienteExisteAsync(dto.ClienteId);

        var novaPlaca = new Placa(dto.Placa);
        await GarantirPlacaDisponivelAsync(novaPlaca, veiculo.Id);

        veiculo.ClienteId = dto.ClienteId;
        veiculo.Placa = novaPlaca;
        veiculo.Marca = dto.Marca.Trim();
        veiculo.Modelo = dto.Modelo.Trim();
        veiculo.Ano = dto.Ano;

        await _repository.AtualizarAsync(veiculo);

        return true;
    }

    public async Task<bool> RemoverAsync(string placa)
    {
        var veiculo = await _repository.ObterPorPlacaAsync(new Placa(placa).Valor);

        if (veiculo is null)
            return false;

        await _repository.RemoverAsync(veiculo.Id);

        return true;
    }
    #endregion

    private async Task GarantirPlacaDisponivelAsync(Placa placa, int? ignorarId = null)
    {
        if (await _repository.PlacaExisteAsync(placa.Valor, ignorarId))
            throw new InvalidOperationException($"Já existe um veículo com a placa '{placa.Valor}'.");
    }

    private async Task GarantirClienteExisteAsync(int clienteId)
    {
        if (await _clienteRepository.ObterPorIdAsync(clienteId) is null)
            throw new KeyNotFoundException($"Cliente {clienteId} não encontrado.");
    }
}
