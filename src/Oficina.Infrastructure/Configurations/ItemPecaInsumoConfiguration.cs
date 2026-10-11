using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oficina.Domain.Entities;

namespace Oficina.Infrastructure.Configurations;

public class ItemPecaInsumoConfiguration : IEntityTypeConfiguration<ItemPecaInsumo>
{
    public void Configure(EntityTypeBuilder<ItemPecaInsumo> builder)
    {
        builder.ToTable("ItensPecaInsumo");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Nome).IsRequired().HasMaxLength(150);
        builder.Property(i => i.Tipo).IsRequired();
        builder.Property(i => i.Quantidade).IsRequired();
        builder.Property(i => i.ValorUnitario).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.ValorTotal).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.Consumido).IsRequired();

        builder.HasOne<Catalogo>()
            .WithMany()
            .HasForeignKey(i => i.CatalogoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
