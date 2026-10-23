using Microsoft.EntityFrameworkCore;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;
using Oficina.Domain.ValueObjects;
using Oficina.Infrastructure.Context;

namespace Oficina.Infrastructure.Repositories;

public class VeiculoRepository : IVeiculoRepository
{
    private readonly OficinaDbContext _context;

    public VeiculoRepository(OficinaDbContext context)
    {
        _context = context;
    }

    #region Basic CRUD
    public async Task<IEnumerable<Veiculo>> ObterTodosAsync()
        => await _context.Veiculos.ToListAsync();

    public async Task<Veiculo?> ObterPorIdAsync(int id)
        => await _context.Veiculos.FindAsync(id);
        
    public async Task<Veiculo?> ObterPorPlacaAsync(string placa)
    {
        var placaBusca = new Placa(placa);

        return await _context.Veiculos.FirstOrDefaultAsync(v => v.Placa == placaBusca);
    }

    public async Task<bool> PlacaExisteAsync(string placa, int? ignorarId = null)
    {
        var placaBusca = new Placa(placa);

        return await _context.Veiculos.AnyAsync(v =>
            v.Placa == placaBusca && (ignorarId == null || v.Id != ignorarId));
    }

    public async Task AdicionarAsync(Veiculo veiculo)
    {
        await _context.Veiculos.AddAsync(veiculo);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Veiculo veiculo)
    {
        _context.Veiculos.Update(veiculo);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(int id)
    {
        var veiculo = await ObterPorIdAsync(id);
        if (veiculo != null)
        {
            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();
        }
    }
    #endregion
}
