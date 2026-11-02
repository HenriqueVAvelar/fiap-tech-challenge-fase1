using Oficina.Domain.Entities;
using Oficina.Domain.Enums;

namespace Oficina.Domain.UnitTests.OrdemServico;

public class OrdemServicoRegrasTests
{
    [Fact]
    public void CriarOrdemServico_DeveAtribuirOsDadosIniciais()
    {
        var antes = DateTime.UtcNow;

        var ordemServico = new Domain.Entities.OrdemServico(3, 5);

        Assert.Equal(3, ordemServico.ClienteId);
        Assert.Equal(5, ordemServico.VeiculoId);
        Assert.InRange(ordemServico.DataCriacao, antes, DateTime.UtcNow);
        Assert.Null(ordemServico.DataInicioDiagnostico);
        Assert.Null(ordemServico.DataConclusaoDiagnostico);
        Assert.Null(ordemServico.DataInicioExecucao);
        Assert.Null(ordemServico.DataFinalizacao);
        Assert.Null(ordemServico.DataEntrega);
        Assert.Null(ordemServico.Orcamento);
        Assert.Empty(ordemServico.ItensServico);
        Assert.Empty(ordemServico.ItensPecaInsumo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CriarOrdemServico_DeveLancarExcecao_QuandoClienteInvalido(int clienteId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Domain.Entities.OrdemServico(clienteId, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CriarOrdemServico_DeveLancarExcecao_QuandoVeiculoInvalido(int veiculoId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Domain.Entities.OrdemServico(1, veiculoId));
    }

    [Fact]
    public void IniciarDiagnostico_DeveLancarExcecao_QuandoOrdemJaEstiverEmDiagnostico()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);
        ordemServico.IniciarDiagnostico();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.IniciarDiagnostico());
    }

    [Fact]
    public void AdicionarServico_DeveIncluirOItemNaOrdem()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);

        ordemServico.AdicionarServico(7, "Alinhamento", 80m);

        var item = Assert.Single(ordemServico.ItensServico);
        Assert.Equal(7, item.ServicoId);
        Assert.Equal("Alinhamento", item.Descricao);
        Assert.Equal(80m, item.Valor);
    }

    [Fact]
    public void AdicionarServico_DevePermitirIncluirVariosServicos_EmDiagnostico()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);
        ordemServico.IniciarDiagnostico();

        ordemServico.AdicionarServico(1, "Troca de óleo", 100m);
        ordemServico.AdicionarServico(2, "Balanceamento", 60m);

        Assert.Equal(2, ordemServico.ItensServico.Count);
    }

    [Fact]
    public void AdicionarServico_DeveLancarExcecao_AposGeracaoDoOrcamento()
    {
        var ordemServico = CriarOrdemComOrcamento();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.AdicionarServico(2, "Balanceamento", 60m));

        Assert.Single(ordemServico.ItensServico);
    }

    [Fact]
    public void AdicionarServico_DeveLancarExcecao_AposDisponibilizacaoDoOrcamento()
    {
        var ordemServico = CriarOrdemComOrcamento();
        ordemServico.DisponibilizarOrcamento();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.AdicionarServico(2, "Balanceamento", 60m));
    }

    [Fact]
    public void AdicionarServico_DeveLancarExcecao_QuandoServicoInvalido()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ordemServico.AdicionarServico(0, "Troca de óleo", 100m));
        Assert.Throws<ArgumentException>(
            () => ordemServico.AdicionarServico(1, " ", 100m));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ordemServico.AdicionarServico(1, "Troca de óleo", -1m));

        Assert.Empty(ordemServico.ItensServico);
    }

    [Fact]
    public void AdicionarPecaInsumo_DeveIncluirOItemNaOrdem_ECalcularOValorTotal()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);
        ordemServico.IniciarDiagnostico();

        ordemServico.AdicionarPecaInsumo(
            9,
            "Filtro de ar",
            EnumsConsumivel.TipoConsumivel.Peca,
            3,
            20m);

        var item = Assert.Single(ordemServico.ItensPecaInsumo);
        Assert.Equal(9, item.CatalogoId);
        Assert.Equal("Filtro de ar", item.Nome);
        Assert.Equal(EnumsConsumivel.TipoConsumivel.Peca, item.Tipo);
        Assert.Equal(3, item.Quantidade);
        Assert.Equal(20m, item.ValorUnitario);
        Assert.Equal(60m, item.ValorTotal);
        Assert.False(item.Consumido);
    }

    [Fact]
    public void AdicionarPecaInsumo_DeveLancarExcecao_QuandoOrdemNaoEstiverEmDiagnostico()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.AdicionarPecaInsumo(
                1,
                "Filtro de ar",
                EnumsConsumivel.TipoConsumivel.Peca,
                1,
                20m));
    }

    [Fact]
    public void AdicionarPecaInsumo_DeveLancarExcecao_AposGeracaoDoOrcamento()
    {
        var ordemServico = CriarOrdemComOrcamento();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.AdicionarPecaInsumo(
                1,
                "Filtro de ar",
                EnumsConsumivel.TipoConsumivel.Peca,
                1,
                20m));

        Assert.Empty(ordemServico.ItensPecaInsumo);
    }

    [Fact]
    public void AdicionarPecaInsumo_DeveLancarExcecao_QuandoItemInvalido()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);
        ordemServico.IniciarDiagnostico();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ordemServico.AdicionarPecaInsumo(0, "Filtro", EnumsConsumivel.TipoConsumivel.Peca, 1, 20m));
        Assert.Throws<ArgumentException>(
            () => ordemServico.AdicionarPecaInsumo(1, "", EnumsConsumivel.TipoConsumivel.Peca, 1, 20m));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ordemServico.AdicionarPecaInsumo(1, "Filtro", EnumsConsumivel.TipoConsumivel.Peca, 0, 20m));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ordemServico.AdicionarPecaInsumo(1, "Filtro", EnumsConsumivel.TipoConsumivel.Peca, 1, -1m));

        Assert.Empty(ordemServico.ItensPecaInsumo);
    }

    [Fact]
    public void ConcluirDiagnostico_DeveRegistrarADataDeConclusao()
    {
        var ordemServico = CriarOrdemComDiagnostico();

        ordemServico.ConcluirDiagnostico();

        Assert.NotNull(ordemServico.DataConclusaoDiagnostico);
        Assert.Equal(
            EnumsOrdemServico.StatusOrdemServico.EmDiagnostico,
            ordemServico.Status);
    }

    [Fact]
    public void ConcluirDiagnostico_DeveLancarExcecao_QuandoOrdemNaoEstiverEmDiagnostico()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.ConcluirDiagnostico());
    }

    [Fact]
    public void ConcluirDiagnostico_DeveLancarExcecao_QuandoJaEstiverConcluido()
    {
        var ordemServico = CriarOrdemComDiagnostico();
        ordemServico.ConcluirDiagnostico();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.ConcluirDiagnostico());
    }

    [Fact]
    public void GerarOrcamento_DeveConsiderarApenasServicos_QuandoNaoHouverPecasInsumos()
    {
        var ordemServico = CriarOrdemComDiagnostico();
        ordemServico.AdicionarServico(2, "Balanceamento", 60m);
        ordemServico.ConcluirDiagnostico();

        ordemServico.GerarOrcamento();

        Assert.Equal(160m, ordemServico.Orcamento!.ValorServicos);
        Assert.Equal(0m, ordemServico.Orcamento.ValorPecasInsumos);
        Assert.Equal(160m, ordemServico.Orcamento.ValorTotal);
    }

    [Fact]
    public void GerarOrcamento_DeveSomarVariosItensDePecasInsumos()
    {
        var ordemServico = CriarOrdemComDiagnostico();
        ordemServico.AdicionarPecaInsumo(1, "Filtro de óleo", EnumsConsumivel.TipoConsumivel.Peca, 2, 25m);
        ordemServico.AdicionarPecaInsumo(2, "Óleo 5W30", EnumsConsumivel.TipoConsumivel.Insumo, 4, 10m);
        ordemServico.ConcluirDiagnostico();

        ordemServico.GerarOrcamento();

        Assert.Equal(90m, ordemServico.Orcamento!.ValorPecasInsumos);
        Assert.Equal(190m, ordemServico.Orcamento.ValorTotal);
    }

    [Fact]
    public void GerarOrcamento_DeveInicializarOOrcamentoComoPendente()
    {
        var antes = DateTime.UtcNow;

        var ordemServico = CriarOrdemComOrcamento();

        var orcamento = ordemServico.Orcamento!;
        Assert.Equal(EnumsOrcamento.StatusOrcamento.Pendente, orcamento.Status);
        Assert.InRange(orcamento.DataGeracao, antes, DateTime.UtcNow);
        Assert.Null(orcamento.DataDisponibilizacao);
        Assert.Null(orcamento.DataDecisao);
    }

    [Fact]
    public void GerarOrcamento_DeveLancarExcecao_QuandoOrdemNaoEstiverEmDiagnostico()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.GerarOrcamento());
    }

    [Fact]
    public void GerarOrcamento_DeveLancarExcecao_QuandoJaExistirOrcamento()
    {
        var ordemServico = CriarOrdemComOrcamento();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.GerarOrcamento());
    }

    [Fact]
    public void GerarOrcamento_DeveLancarExcecao_QuandoNaoHouverServicos()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);
        ordemServico.IniciarDiagnostico();
        ordemServico.ConcluirDiagnostico();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.GerarOrcamento());

        Assert.Null(ordemServico.Orcamento);
    }

    [Fact]
    public void DisponibilizarOrcamento_DeveLancarExcecao_QuandoOrdemNaoEstiverEmDiagnostico()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.DisponibilizarOrcamento());
    }

    [Fact]
    public void DisponibilizarOrcamento_DeveLancarExcecao_QuandoOrcamentoNaoTiverSidoGerado()
    {
        var ordemServico = CriarOrdemComDiagnostico();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.DisponibilizarOrcamento());
    }

    [Fact]
    public void DisponibilizarOrcamento_DeveLancarExcecao_QuandoJaTiverSidoDisponibilizado()
    {
        var ordemServico = CriarOrdemComOrcamentoDisponibilizado();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.DisponibilizarOrcamento());
    }

    [Fact]
    public void AprovarOrcamento_DeveRegistrarADecisao_SemAlterarOStatusDaOrdem()
    {
        var ordemServico = CriarOrdemComOrcamentoDisponibilizado();

        ordemServico.AprovarOrcamento();

        Assert.Equal(EnumsOrcamento.StatusOrcamento.Aprovado, ordemServico.Orcamento!.Status);
        Assert.NotNull(ordemServico.Orcamento.DataDecisao);
        Assert.Equal(
            EnumsOrdemServico.StatusOrdemServico.AguardandoAprovacao,
            ordemServico.Status);
    }

    [Fact]
    public void AprovarOrcamento_DeveLancarExcecao_QuandoOrdemNaoEstiverAguardandoAprovacao()
    {
        var ordemServico = CriarOrdemComOrcamento();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.AprovarOrcamento());
    }

    [Fact]
    public void AprovarOrcamento_DeveLancarExcecao_QuandoOrcamentoJaTiverSidoAprovado()
    {
        var ordemServico = CriarOrdemComOrcamentoDisponibilizado();
        ordemServico.AprovarOrcamento();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.AprovarOrcamento());
    }

    [Fact]
    public void AprovarOrcamento_DeveLancarExcecao_QuandoOrcamentoJaTiverSidoRejeitado()
    {
        var ordemServico = CriarOrdemComOrcamentoDisponibilizado();
        ordemServico.RejeitarOrcamento();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.AprovarOrcamento());

        Assert.Equal(EnumsOrcamento.StatusOrcamento.Rejeitado, ordemServico.Orcamento!.Status);
    }

    [Fact]
    public void RejeitarOrcamento_DeveRegistrarADecisao_SemAlterarOStatusDaOrdem()
    {
        var ordemServico = CriarOrdemComOrcamentoDisponibilizado();

        ordemServico.RejeitarOrcamento();

        Assert.NotNull(ordemServico.Orcamento!.DataDecisao);
        Assert.Equal(
            EnumsOrdemServico.StatusOrdemServico.AguardandoAprovacao,
            ordemServico.Status);
    }

    [Fact]
    public void RejeitarOrcamento_DeveLancarExcecao_QuandoOrdemNaoEstiverAguardandoAprovacao()
    {
        var ordemServico = CriarOrdemComOrcamento();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.RejeitarOrcamento());
    }

    [Fact]
    public void RejeitarOrcamento_DeveLancarExcecao_QuandoOrcamentoJaTiverSidoRejeitado()
    {
        var ordemServico = CriarOrdemComOrcamentoDisponibilizado();
        ordemServico.RejeitarOrcamento();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.RejeitarOrcamento());
    }

    [Fact]
    public void RejeitarOrcamento_DeveLancarExcecao_QuandoOrcamentoJaTiverSidoAprovado()
    {
        var ordemServico = CriarOrdemComOrcamentoDisponibilizado();
        ordemServico.AprovarOrcamento();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.RejeitarOrcamento());

        Assert.Equal(EnumsOrcamento.StatusOrcamento.Aprovado, ordemServico.Orcamento!.Status);
    }

    [Fact]
    public void IniciarExecucao_DeveLancarExcecao_QuandoOrdemNaoEstiverAguardandoAprovacao()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.IniciarExecucao());
    }

    [Fact]
    public void IniciarExecucao_DeveLancarExcecao_QuandoOrdemJaEstiverEmExecucao()
    {
        var ordemServico = CriarOrdemEmExecucao();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.IniciarExecucao());
    }

    [Fact]
    public void Finalizar_DeveLancarExcecao_QuandoOrdemNaoEstiverEmExecucao()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.Finalizar());
    }

    [Fact]
    public void Finalizar_DeveLancarExcecao_QuandoOrdemJaEstiverFinalizada()
    {
        var ordemServico = CriarOrdemEmExecucao();
        ordemServico.Finalizar();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.Finalizar());
    }

    [Fact]
    public void Entregar_DeveLancarExcecao_QuandoOrdemJaTiverSidoEntregue()
    {
        var ordemServico = CriarOrdemEmExecucao();
        ordemServico.Finalizar();
        ordemServico.Entregar();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.Entregar());
    }

    [Fact]
    public void FluxoCompleto_DeveRegistrarTodasAsDatasNaOrdem()
    {
        var ordemServico = CriarOrdemEmExecucao();
        ordemServico.Finalizar();
        ordemServico.Entregar();

        Assert.NotNull(ordemServico.DataInicioDiagnostico);
        Assert.NotNull(ordemServico.DataConclusaoDiagnostico);
        Assert.NotNull(ordemServico.DataInicioExecucao);
        Assert.NotNull(ordemServico.DataFinalizacao);
        Assert.NotNull(ordemServico.DataEntrega);
        Assert.NotNull(ordemServico.Orcamento!.DataDisponibilizacao);
        Assert.NotNull(ordemServico.Orcamento.DataDecisao);
    }

    [Fact]
    public void RegistrarConsumoPecasInsumos_DeveLancarExcecao_QuandoListaForNula()
    {
        var ordemServico = CriarOrdemEmExecucao();

        Assert.Throws<ArgumentNullException>(
            () => ordemServico.RegistrarConsumoPecasInsumos(null!));
    }

    private static Domain.Entities.OrdemServico CriarOrdemComDiagnostico()
    {
        var ordemServico =
            new Domain.Entities.OrdemServico(1, 1);

        ordemServico.AdicionarServico(
            1,
            "Troca de óleo",
            100m);

        ordemServico.IniciarDiagnostico();

        return ordemServico;
    }

    private static Domain.Entities.OrdemServico CriarOrdemComOrcamento()
    {
        var ordemServico =
            CriarOrdemComDiagnostico();

        ordemServico.ConcluirDiagnostico();
        ordemServico.GerarOrcamento();

        return ordemServico;
    }

    private static Domain.Entities.OrdemServico CriarOrdemComOrcamentoDisponibilizado()
    {
        var ordemServico =
            CriarOrdemComOrcamento();

        ordemServico.DisponibilizarOrcamento();

        return ordemServico;
    }

    private static Domain.Entities.OrdemServico CriarOrdemEmExecucao()
    {
        var ordemServico =
            CriarOrdemComOrcamentoDisponibilizado();

        ordemServico.AprovarOrcamento();
        ordemServico.IniciarExecucao();

        return ordemServico;
    }
}
