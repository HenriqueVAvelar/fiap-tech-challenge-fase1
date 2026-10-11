using Oficina.Domain.Entities;
using Oficina.Domain.Enums;
using Ordem = Oficina.Domain.Entities.OrdemServico;

namespace Oficina.Domain.UnitTests.Entities;

public class OrdemServicoConsumoTests
{
    // Cria uma OS "Em execução" com um item de peça/insumo para cada quantidade informada.
    // Os itens recebem os ids 1, 2, 3... (no banco o id é gerado na gravação).
    private static Ordem CriarOrdemEmExecucao(params int[] quantidades)
    {
        var ordem = new Ordem(1, 1);

        ordem.IniciarDiagnostico();
        ordem.AdicionarServico(1, "Troca de óleo", 100m);

        for (var i = 0; i < quantidades.Length; i++)
            ordem.AdicionarPecaInsumo(10 + i, $"Peça {i + 1}", EnumsConsumivel.TipoConsumivel.Peca, quantidades[i], 50m);

        var id = 1;
        foreach (var item in ordem.ItensPecaInsumo)
            typeof(ItemPecaInsumo).GetProperty("Id")!.SetValue(item, id++);

        ordem.ConcluirDiagnostico();
        ordem.GerarOrcamento();
        ordem.DisponibilizarOrcamento();
        ordem.AprovarOrcamento();
        ordem.IniciarExecucao();

        return ordem;
    }

    [Fact]
    public void RegistrarConsumo_DeveMarcarTodosOsItensInformados_EDevolverOsItens()
    {
        var ordem = CriarOrdemEmExecucao(2, 3, 4);

        var itens = ordem.RegistrarConsumoPecasInsumos(new[] { 1, 3 });

        Assert.Equal(new[] { 1, 3 }, itens.Select(i => i.Id));
        Assert.All(itens, i => Assert.True(i.Consumido));
        Assert.False(ordem.ItensPecaInsumo.Single(i => i.Id == 2).Consumido);
    }

    [Fact]
    public void RegistrarConsumo_DeveIgnorarIdsRepetidos()
    {
        var ordem = CriarOrdemEmExecucao(2);

        var itens = ordem.RegistrarConsumoPecasInsumos(new[] { 1, 1 });

        Assert.Single(itens);
    }

    [Fact]
    public void RegistrarConsumo_DeveLancarExcecao_QuandoAListaEstaVazia()
    {
        var ordem = CriarOrdemEmExecucao(2);

        Assert.Throws<ArgumentException>(() => ordem.RegistrarConsumoPecasInsumos(Array.Empty<int>()));
    }

    [Fact]
    public void RegistrarConsumo_DeveLancarExcecao_QuandoAlgumItemNaoExiste_ENaoMarcaNenhum()
    {
        var ordem = CriarOrdemEmExecucao(2, 3);

        Assert.Throws<KeyNotFoundException>(() => ordem.RegistrarConsumoPecasInsumos(new[] { 1, 99 }));

        Assert.All(ordem.ItensPecaInsumo, i => Assert.False(i.Consumido));
    }

    [Fact]
    public void RegistrarConsumo_DeveLancarExcecao_QuandoAlgumItemJaFoiConsumido_ENaoMarcaOsDemais()
    {
        var ordem = CriarOrdemEmExecucao(2, 3);
        ordem.RegistrarConsumoPecasInsumos(new[] { 1 });

        Assert.Throws<InvalidOperationException>(() => ordem.RegistrarConsumoPecasInsumos(new[] { 2, 1 }));

        Assert.False(ordem.ItensPecaInsumo.Single(i => i.Id == 2).Consumido);
    }

    [Fact]
    public void RegistrarConsumo_DeveLancarExcecao_QuandoAOrdemNaoEstaEmExecucao()
    {
        var ordem = new Ordem(1, 1);
        ordem.IniciarDiagnostico();
        ordem.AdicionarPecaInsumo(10, "Peça 1", EnumsConsumivel.TipoConsumivel.Peca, 2, 50m);

        Assert.Throws<InvalidOperationException>(() => ordem.RegistrarConsumoPecasInsumos(new[] { 0 }));
    }

    [Fact]
    public void ItemIncluido_NaoDeveNascerConsumido()
    {
        var ordem = new Ordem(1, 1);
        ordem.IniciarDiagnostico();
        ordem.AdicionarPecaInsumo(10, "Peça 1", EnumsConsumivel.TipoConsumivel.Peca, 2, 50m);

        Assert.False(Assert.Single(ordem.ItensPecaInsumo).Consumido);
    }
}
