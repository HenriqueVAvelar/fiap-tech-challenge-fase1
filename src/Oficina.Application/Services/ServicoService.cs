using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oficina.Application.Services;

public class ServicoService : IServicoService
{
    private readonly IServicoRepository _repository;

    public ServicoService(IServicoRepository repository)
    {
        _repository = repository;
    }

    #region Basic CRUD
    public async Task<IEnumerable<Servico>> ObterTodosAsync()
        => await _repository.ObterTodosAsync();

    public async Task<Servico?> ObterPorIdAsync(int id)
        => await _repository.ObterPorIdAsync(id);

    public async Task CriarAsync(Servico servico)
        => await _repository.AdicionarAsync(servico);

    public async Task AtualizarAsync(Servico servico)
        => await _repository.AtualizarAsync(servico);

    public async Task RemoverAsync(int id)
        => await _repository.RemoverAsync(id);
    #endregion
}
