using Oficina.Domain.Entities;
using Oficina.Domain.Enums;

namespace Oficina.Domain.UnitTests.Entities;

public class CatalogoDescricaoTests
{
    [Fact]
    public void Construtor_DeveManterDescricaoNula_QuandoNaoInformada()
    {
        var catalogo = new Catalogo("Óleo", 40m, EnumsConsumivel.TipoConsumivel.Insumo, true);

        Assert.Null(catalogo.Descricao);
    }

    [Fact]
    public void Atualizar_DevePermitirRemoverADescricao()
    {
        var catalogo = new Catalogo("Óleo", 40m, EnumsConsumivel.TipoConsumivel.Insumo, true, "Sintético");

        catalogo.Atualizar("Óleo", 40m, EnumsConsumivel.TipoConsumivel.Insumo, true, null);

        Assert.Null(catalogo.Descricao);
    }

    [Fact]
    public void Atualizar_DevePermitirPrecoZero()
    {
        var catalogo = new Catalogo("Óleo", 40m, EnumsConsumivel.TipoConsumivel.Insumo, true);

        catalogo.Atualizar("Óleo", 0m, EnumsConsumivel.TipoConsumivel.Insumo, true, null);

        Assert.Equal(0m, catalogo.Preco);
    }
}
