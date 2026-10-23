using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oficina.Domain.Entities;
using Oficina.Domain.ValueObjects;

namespace Oficina.Infrastructure.Configurations;

public class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> builder)
    {
        builder.ToTable("Veiculos");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Placa)
            .IsRequired()
            .HasMaxLength(7)
            .HasConversion(placa => placa.Valor, valor => new Placa(valor));

        // Uma placa não pode pertencer a dois veículos
        builder.HasIndex(v => v.Placa)
            .IsUnique();

        builder.Property(v => v.Marca)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Modelo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Ano)
            .IsRequired();

        builder.HasOne(v => v.Cliente)
            .WithMany()
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
