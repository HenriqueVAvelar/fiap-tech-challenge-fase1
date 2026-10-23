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

    public async Task<Cliente?> ObterPorDocumentoAsync(string numero)
        => await _context.Clientes.FirstOrDefaultAsync(c => c.Documento.Numero == numero);

    public async Task<bool> DocumentoExisteAsync(string numero, int? ignorarId = null)
        => await _context.Clientes.AnyAsync(c =>
            c.Documento.Numero == numero && (ignorarId == null || c.Id != ignorarId));

    public async Task AdicionarAsync(Cliente cliente)
    {
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(int id)
    {
        var cliente = await ObterPorIdAsync(id);
        if (cliente != null)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
        }
    }
    #endregion
}
