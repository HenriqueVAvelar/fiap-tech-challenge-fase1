using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oficina.Domain.Entities;

namespace Oficina.Infrastructure.Configurations;

public class OrcamentoConfiguration : IEntityTypeConfiguration<Orcamento>
{
    public void Configure(EntityTypeBuilder<Orcamento> builder)
    {
        builder.ToTable("Orcamentos");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.ValorServicos)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.ValorPecasInsumos)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.ValorTotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.Status)
            .IsRequired();

        builder.Property(o => o.DataGeracao)
            .IsRequired();

        builder.Property(o => o.DataDisponibilizacao);

        builder.Property(o => o.DataDecisao);
    }
}
