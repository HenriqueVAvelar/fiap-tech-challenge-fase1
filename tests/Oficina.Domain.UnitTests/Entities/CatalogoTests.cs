using Oficina.Domain.Entities;
using Oficina.Domain.Enums;

namespace Oficina.Domain.UnitTests.Entities;

public class CatalogoTests
{
    [Fact]
    public void Construtor_DeveAtribuirOsDados()
    {
        var catalogo = new Catalogo("Alternador", 900m, EnumsConsumivel.TipoConsumivel.Peca, true, "Descrição");

        Assert.Equal("Alternador", catalogo.Nome);
        Assert.Equal(900m, catalogo.Preco);
        Assert.Equal(EnumsConsumivel.TipoConsumivel.Peca, catalogo.Tipo);
        Assert.True(catalogo.Ativo);
        Assert.Equal("Descrição", catalogo.Descricao);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Construtor_DeveLancarExcecao_QuandoNomeVazio(string nome)
    {
        Assert.Throws<ArgumentException>(() => new Catalogo(nome, 10m, EnumsConsumivel.TipoConsumivel.Insumo, true));
    }

    [Fact]
    public void Construtor_DeveLancarExcecao_QuandoPrecoNegativo()
    {
        Assert.Throws<ArgumentException>(() => new Catalogo("Óleo", -0.01m, EnumsConsumivel.TipoConsumivel.Insumo, true));
    }

    [Fact]
    public void Construtor_DevePermitirPrecoZero()
    {
        var catalogo = new Catalogo("Brinde", 0m, EnumsConsumivel.TipoConsumivel.Insumo, true);

        Assert.Equal(0m, catalogo.Preco);
    }

    [Fact]
    public void Atualizar_DeveAlterarOsDados()
    {
        var catalogo = new Catalogo("Óleo", 40m, EnumsConsumivel.TipoConsumivel.Insumo, true);

        catalogo.Atualizar("Bateria", 500m, EnumsConsumivel.TipoConsumivel.Peca, false, "Nova");

        Assert.Equal("Bateria", catalogo.Nome);
        Assert.Equal(500m, catalogo.Preco);
        Assert.Equal(EnumsConsumivel.TipoConsumivel.Peca, catalogo.Tipo);
        Assert.False(catalogo.Ativo);
        Assert.Equal("Nova", catalogo.Descricao);
    }

    [Fact]
    public void Atualizar_NaoDeveAlterarNada_QuandoDadosInvalidos()
    {
        var catalogo = new Catalogo("Óleo", 40m, EnumsConsumivel.TipoConsumivel.Insumo, true);

        Assert.Throws<ArgumentException>(() =>
            catalogo.Atualizar("", 50m, EnumsConsumivel.TipoConsumivel.Peca, false, null));
        Assert.Throws<ArgumentException>(() =>
            catalogo.Atualizar("Outro", -1m, EnumsConsumivel.TipoConsumivel.Peca, false, null));

        Assert.Equal("Óleo", catalogo.Nome);
        Assert.Equal(40m, catalogo.Preco);
    }
}
