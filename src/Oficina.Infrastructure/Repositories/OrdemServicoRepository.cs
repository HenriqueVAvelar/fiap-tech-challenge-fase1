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
        => await _context.OrdensServico.ToListAsync();

    public async Task<OrdemServico?> ObterPorIdAsync(int id)
        => await _context.OrdensServico.FindAsync(id);

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
}
