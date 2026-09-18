using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oficina.Application.Services;

public class OrdemServicoService : IOrdemServicoService
{
    private readonly IOrdemServicoRepository _repository;

    public OrdemServicoService(IOrdemServicoRepository repository)
    {
        _repository = repository;
    }

    #region Basic CRUD
    public async Task<IEnumerable<OrdemServico>> ObterTodosAsync()
        => await _repository.ObterTodosAsync();

    public async Task<OrdemServico?> ObterPorIdAsync(int id)
        => await _repository.ObterPorIdAsync(id);

    public async Task CriarAsync(OrdemServico ordemServico)
        => await _repository.AdicionarAsync(ordemServico);

    public async Task AtualizarAsync(OrdemServico ordemServico)
        => await _repository.AtualizarAsync(ordemServico);

    public async Task RemoverAsync(int id)
        => await _repository.RemoverAsync(id);
    #endregion
}
