using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oficina.Domain.Entities;

namespace Oficina.Infrastructure.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(150);

        // Value object Documento gravado nas colunas DocumentoNumero e DocumentoTipo da tabela Clientes
        builder.OwnsOne(c => c.Documento, documento =>
        {
            documento.Property(d => d.Numero)
                .HasColumnName("DocumentoNumero")
                .HasMaxLength(14)
                .IsRequired();

            documento.Property(d => d.Tipo)
                .HasColumnName("DocumentoTipo")
                .IsRequired();

            // Um documento não pode pertencer a dois clientes
            documento.HasIndex(d => d.Numero)
                .IsUnique();
        });

        builder.Navigation(c => c.Documento)
            .IsRequired();
    }
}
