using Microsoft.EntityFrameworkCore;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;
using Oficina.Infrastructure.Context;

namespace Oficina.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly OficinaDbContext _context;

    public UsuarioRepository(OficinaDbContext context)
    {
        _context = context;
    }

    #region Basic CRUD
    public async Task<IEnumerable<Usuario>> ObterTodosAsync()
        => await _context.Usuarios.AsNoTracking().ToListAsync();

    public async Task<Usuario?> ObterPorIdAsync(int id)
        => await _context.Usuarios.FindAsync(id);

    public async Task<Usuario?> ObterPorUserNameAsync(string userName)
    {
        var normalizado = userName.Trim().ToLower();

        return await _context.Usuarios.FirstOrDefaultAsync(u => u.UserName.ToLower() == normalizado);
    }

    public async Task<bool> UserNameExisteAsync(string userName, int? ignorarId = null)
    {
        var normalizado = userName.Trim().ToLower();

        return await _context.Usuarios.AnyAsync(u =>
            u.UserName.ToLower() == normalizado && (ignorarId == null || u.Id != ignorarId));
    }

    public async Task AdicionarAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }
    }
    #endregion
}
