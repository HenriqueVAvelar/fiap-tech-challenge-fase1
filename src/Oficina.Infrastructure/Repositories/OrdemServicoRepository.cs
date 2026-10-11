using Microsoft.EntityFrameworkCore;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;
using Oficina.Infrastructure.Context;

namespace Oficina.Infrastructure.Repositories;

public class OrdemServicoRepository : IOrdemServicoRepository
{
    private readonly OficinaDbContext _context;

    public OrdemServicoRepository(OficinaDbContext context)
    {
        _context = context;
    }

    #region Basic CRUD
    public async Task<IEnumerable<OrdemServico>> ObterTodosAsync()
        => await ComDetalhes().ToListAsync();

    public async Task<OrdemServico?> ObterPorIdAsync(int id)
        => await ComDetalhes().FirstOrDefaultAsync(o => o.Id == id);

    public async Task AdicionarAsync(OrdemServico ordemServico)
    {
        await _context.OrdensServico.AddAsync(ordemServico);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(OrdemServico ordemServico)
    {
        _context.OrdensServico.Update(ordemServico);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(int id)
    {
        var ordemServico = await ObterPorIdAsync(id);
        if (ordemServico != null)
        {
            _context.OrdensServico.Remove(ordemServico);
            await _context.SaveChangesAsync();
        }
    }
    #endregion

    // Carrega os itens e o orçamento junto com a ordem; sem isso as regras do domínio
    // (ex.: "já existe orçamento") enxergariam a ordem incompleta.
    private IQueryable<OrdemServico> ComDetalhes()
        => _context.OrdensServico
            .Include(o => o.ItensServico)
            .Include(o => o.ItensPecaInsumo)
            .Include(o => o.Orcamento)
            .AsSplitQuery();
}
