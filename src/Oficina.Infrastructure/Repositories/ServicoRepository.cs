using Microsoft.EntityFrameworkCore;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;
using Oficina.Infrastructure.Context;

namespace Oficina.Infrastructure.Repositories;

public class ServicoRepository : IServicoRepository
{
    private readonly OficinaDbContext _context;

    public ServicoRepository(OficinaDbContext context)
    {
        _context = context;
    }

    #region Basic CRUD
    public async Task<IEnumerable<Servico>> ObterTodosAsync()
        => await _context.Servicos.ToListAsync();

    public async Task<Servico?> ObterPorIdAsync(int id)
        => await _context.Servicos.FindAsync(id);

    public async Task AdicionarAsync(Servico servico)
    {
        await _context.Servicos.AddAsync(servico);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Servico servico)
    {
        _context.Servicos.Update(servico);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(int id)
    {
        var servico = await ObterPorIdAsync(id);
        if (servico != null)
        {
            _context.Servicos.Remove(servico);
            await _context.SaveChangesAsync();
        }
    }
    #endregion
}
