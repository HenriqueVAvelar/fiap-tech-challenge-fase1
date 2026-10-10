using Oficina.Domain.Entities;
using Oficina.Domain.Enums;

namespace Oficina.Domain.UnitTests.OrdemServico;

public class OrdemServicoTests
{
    [Fact]
    public void CriarOrdemServico_DeveIniciarComStatusRecebida()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);

        Assert.Equal(
            EnumsOrdemServico.StatusOrdemServico.Recebida,
            ordemServico.Status);
    }

    [Fact]
    public void IniciarDiagnostico_DeveAlterarStatusParaEmDiagnostico()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);

        ordemServico.IniciarDiagnostico();

        Assert.Equal(
            EnumsOrdemServico.StatusOrdemServico.EmDiagnostico,
            ordemServico.Status);

        Assert.NotNull(ordemServico.DataInicioDiagnostico);
    }

    [Fact]
    public void GerarOrcamento_DeveCalcularServicosEPecasInsumos()
    {
        var ordemServico = CriarOrdemComDiagnostico();

        ordemServico.AdicionarPecaInsumo(
            1,
            "Filtro de óleo",
            EnumsConsumivel.TipoConsumivel.Peca,
            2,
            25m);

        ordemServico.ConcluirDiagnostico();

        ordemServico.GerarOrcamento();

        Assert.NotNull(ordemServico.Orcamento);

        Assert.Equal(
            100m,
            ordemServico.Orcamento.ValorServicos);

        Assert.Equal(
            50m,
            ordemServico.Orcamento.ValorPecasInsumos);

        Assert.Equal(
            150m,
            ordemServico.Orcamento.ValorTotal);

        Assert.Equal(
            EnumsOrcamento.StatusOrcamento.Pendente,
            ordemServico.Orcamento.Status);
    }

    [Fact]
    public void DisponibilizarOrcamento_DeveAlterarStatusParaAguardandoAprovacao()
    {
        var ordemServico = CriarOrdemComOrcamento();

        ordemServico.DisponibilizarOrcamento();

        Assert.Equal(
            EnumsOrdemServico.StatusOrdemServico.AguardandoAprovacao,
            ordemServico.Status);

        Assert.NotNull(
            ordemServico.Orcamento!.DataDisponibilizacao);
    }

    [Fact]
    public void AprovarOrcamento_DevePermitirInicioDaExecucao()
    {
        var ordemServico =
            CriarOrdemComOrcamentoDisponibilizado();

        ordemServico.AprovarOrcamento();
        ordemServico.IniciarExecucao();

        Assert.Equal(
            EnumsOrcamento.StatusOrcamento.Aprovado,
            ordemServico.Orcamento!.Status);

        Assert.Equal(
            EnumsOrdemServico.StatusOrdemServico.EmExecucao,
            ordemServico.Status);

        Assert.NotNull(ordemServico.DataInicioExecucao);
    }

    [Fact]
    public void RejeitarOrcamento_NaoDevePermitirInicioDaExecucao()
    {
        var ordemServico =
            CriarOrdemComOrcamentoDisponibilizado();

        ordemServico.RejeitarOrcamento();

        Assert.Equal(
            EnumsOrcamento.StatusOrcamento.Rejeitado,
            ordemServico.Orcamento!.Status);

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.IniciarExecucao());
    }

    [Fact]
    public void IniciarExecucao_DeveLancarExcecao_SemAprovacaoDoOrcamento()
    {
        var ordemServico =
            CriarOrdemComOrcamentoDisponibilizado();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.IniciarExecucao());
    }

    [Fact]
    public void FinalizarEEntregar_DeveRespeitarFluxoDeStatus()
    {
        var ordemServico =
            CriarOrdemEmExecucao();

        ordemServico.Finalizar();

        Assert.Equal(
            EnumsOrdemServico.StatusOrdemServico.Finalizada,
            ordemServico.Status);

        Assert.NotNull(ordemServico.DataFinalizacao);

        ordemServico.Entregar();

        Assert.Equal(
            EnumsOrdemServico.StatusOrdemServico.Entregue,
            ordemServico.Status);

        Assert.NotNull(ordemServico.DataEntrega);
    }

    [Fact]
    public void Entregar_DeveLancarExcecao_QuandoOrdemNaoEstiverFinalizada()
    {
        var ordemServico = new Domain.Entities.OrdemServico(1, 1);

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.Entregar());
    }

    [Fact]
    public void GerarOrcamento_DeveLancarExcecao_QuandoDiagnosticoNaoEstiverConcluido()
    {
        var ordemServico = CriarOrdemComDiagnostico();

        Assert.Throws<InvalidOperationException>(
            () => ordemServico.GerarOrcamento());
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