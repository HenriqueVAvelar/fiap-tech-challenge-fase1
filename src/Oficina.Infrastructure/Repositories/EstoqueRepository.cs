using Microsoft.EntityFrameworkCore;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;
using Oficina.Infrastructure.Context;

namespace Oficina.Infrastructure.Repositories;

public class EstoqueRepository : IEstoqueRepository
{
    private readonly OficinaDbContext _context;

    public EstoqueRepository(OficinaDbContext context)
    {
        _context = context;
    }

    #region Basic CRUD
    public async Task<IEnumerable<Estoque>> ObterTodosAsync()
        => await _context.Estoques.ToListAsync();

    public async Task<Estoque?> ObterPorIdAsync(int id)
        => await _context.Estoques.FindAsync(id);

    public async Task AdicionarAsync(Estoque estoque)
    {
        await _context.Estoques.AddAsync(estoque);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Estoque estoque)
    {
        _context.Estoques.Update(estoque);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(int id)
    {
        var estoque = await ObterPorIdAsync(id);
        if (estoque != null)
        {
            _context.Estoques.Remove(estoque);
            await _context.SaveChangesAsync();
        }
    }
    #endregion
}
