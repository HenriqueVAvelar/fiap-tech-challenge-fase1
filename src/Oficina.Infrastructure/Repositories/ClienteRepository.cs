using Microsoft.EntityFrameworkCore;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;
using Oficina.Infrastructure.Context;

namespace Oficina.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly OficinaDbContext _context;

    public ClienteRepository(OficinaDbContext context)
    {
        _context = context;
    }

    #region Basic CRUD
    public async Task<IEnumerable<Cliente>> ObterTodosAsync()
        => await _context.Clientes.ToListAsync();

    public async Task<Cliente?> ObterPorIdAsync(int id)
        => await _context.Clientes.FindAsync(id);

    public async Task<bool> DocumentoExisteAsync(string numero, int? ignorarId = null)
        => await _context.Clientes.AnyAsync(c =>
            c.Documento.Numero == numero && (ignorarId == null || c.Id != ignorarId));

    public async Task AdicionarAsync(Cliente ciente)
    {
        await _context.Clientes.AddAsync(ciente);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Cliente ciente)
    {
        _context.Clientes.Update(ciente);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(int id)
    {
        var ciente = await ObterPorIdAsync(id);
        if (ciente != null)
        {
            _context.Clientes.Remove(ciente);
            await _context.SaveChangesAsync();
        }
    }
    #endregion
}
