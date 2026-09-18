using Microsoft.EntityFrameworkCore;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;
using Oficina.Infrastructure.Context;

namespace Oficina.Infrastructure.Repositories;

public class CatalogoRepository : ICatalogoRepository
{
    private readonly OficinaDbContext _context;

    public CatalogoRepository(OficinaDbContext context)
    {
        _context = context;
    }

    #region Basic CRUD
    public async Task<IEnumerable<Catalogo>> ObterTodosAsync()
        => await _context.Catalogos.ToListAsync();

    public async Task<Catalogo?> ObterPorIdAsync(int id)
        => await _context.Catalogos.FindAsync(id);

    public async Task AdicionarAsync(Catalogo catalogo)
    {
        await _context.Catalogos.AddAsync(catalogo);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Catalogo catalogo)
    {
        _context.Catalogos.Update(catalogo);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(int id)
    {
        var item = await ObterPorIdAsync(id);
        if (item != null)
        {
            _context.Catalogos.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
    #endregion
}
