using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oficina.Domain.Entities;

namespace Oficina.Infrastructure.Configurations;

public class EstoqueConfiguration : IEntityTypeConfiguration<Estoque>
{
    public void Configure(EntityTypeBuilder<Estoque> builder)
    {
        builder.ToTable("Estoques");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Quantidade).IsRequired();
                
        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasOne(e => e.Catalogo)
            .WithMany()
            .HasForeignKey(e => e.CatalogoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Um registro de estoque por item de catálogo
        builder.HasIndex(e => e.CatalogoId).IsUnique();
    }
}
