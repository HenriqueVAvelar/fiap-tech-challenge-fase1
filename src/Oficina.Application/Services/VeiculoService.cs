using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oficina.Application.Services;

public class VeiculoService : IVeiculoService
{
    private readonly IVeiculoRepository _repository;

    public VeiculoService(IVeiculoRepository repository)
    {
        _repository = repository;
    }

    #region Basic CRUD
    public async Task<IEnumerable<Veiculo>> ObterTodosAsync()
        => await _repository.ObterTodosAsync();

    public async Task<Veiculo?> ObterPorIdAsync(int id)
        => await _repository.ObterPorIdAsync(id);

    public async Task CriarAsync(Veiculo veiculo)
        => await _repository.AdicionarAsync(veiculo);

    public async Task AtualizarAsync(Veiculo veiculo)
        => await _repository.AtualizarAsync(veiculo);

    public async Task RemoverAsync(int id)
        => await _repository.RemoverAsync(id);
    #endregion
}
