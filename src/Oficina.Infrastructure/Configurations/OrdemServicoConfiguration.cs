using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oficina.Domain.Entities;

namespace Oficina.Infrastructure.Configurations;

public class OrdemServicoConfiguration : IEntityTypeConfiguration<OrdemServico>
{
    public void Configure(EntityTypeBuilder<OrdemServico> builder)
    {
        builder.ToTable("OrdensServico");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Status)
            .IsRequired();

        builder.Property(o => o.DataCriacao)
            .IsRequired();

        builder.Property(o => o.DataInicioDiagnostico);

        builder.Property(o => o.DataConclusaoDiagnostico);

        builder.Property(o => o.DataInicioExecucao);

        builder.Property(o => o.DataFinalizacao);

        builder.Property(o => o.DataEntrega);

        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(o => o.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Veiculo>()
            .WithMany()
            .HasForeignKey(o => o.VeiculoId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(o => o.ItensServico)
            .WithOne()
            .HasForeignKey(i => i.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.ItensServico)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(o => o.ItensPecaInsumo)
            .WithOne()
            .HasForeignKey(i => i.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.ItensPecaInsumo)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Uma OS possui no máximo um orçamento
        builder.HasOne(o => o.Orcamento)
            .WithOne()
            .HasForeignKey<Orcamento>(orc => orc.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
