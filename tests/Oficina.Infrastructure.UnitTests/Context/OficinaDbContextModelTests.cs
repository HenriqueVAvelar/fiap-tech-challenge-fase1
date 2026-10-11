using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Oficina.Domain.Entities;
using Oficina.Infrastructure.Context;

namespace Oficina.Infrastructure.UnitTests.Context;

public class OficinaDbContextModelTests
{
    // Apenas monta o modelo do EF; não abre conexão com o banco.
    private static IModel CriarModelo()
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseNpgsql("Host=localhost;Database=modelo_teste")
            .Options;

        using var context = new OficinaDbContext(options);

        return context.GetService<IDesignTimeModel>().Model;
    }

    [Theory]
    [InlineData(typeof(Usuario), "Usuarios")]
    [InlineData(typeof(Catalogo), "Catalogos")]
    [InlineData(typeof(Cliente), "Clientes")]
    [InlineData(typeof(Veiculo), "Veiculos")]
    [InlineData(typeof(Servico), "Servicos")]
    [InlineData(typeof(Estoque), "Estoques")]
    [InlineData(typeof(OrdemServico), "OrdensServico")]
    [InlineData(typeof(ItemServico), "ItensServico")]
    [InlineData(typeof(ItemPecaInsumo), "ItensPecaInsumo")]
    [InlineData(typeof(Orcamento), "Orcamentos")]
    public void Modelo_DeveMapearCadaEntidadeParaSuaTabela(Type entidade, string tabela)
    {
        var tipo = CriarModelo().FindEntityType(entidade);

        Assert.NotNull(tipo);
        Assert.Equal(tabela, tipo!.GetTableName());
    }
}
