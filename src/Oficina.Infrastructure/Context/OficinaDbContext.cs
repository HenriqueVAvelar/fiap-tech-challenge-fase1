using Microsoft.EntityFrameworkCore;
using Oficina.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oficina.Infrastructure.Context;

public class OficinaDbContext : DbContext
{
    public OficinaDbContext(DbContextOptions<OficinaDbContext> options) : base(options)
    {
    }

    public DbSet<Catalogo> Catalogos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Estoque> Estoques { get; set; }
    public DbSet<OrdemServico> OrdensServico { get; set; }
    public DbSet<Servico> Servicos { get; set; }
    public DbSet<Veiculo> Veiculos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OficinaDbContext).Assembly);
    }
}
