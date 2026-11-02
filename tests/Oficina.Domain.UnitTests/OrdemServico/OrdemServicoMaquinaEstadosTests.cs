using Oficina.Domain.Enums;

namespace Oficina.Domain.UnitTests.OrdemServico;

public class OrdemServicoMaquinaEstadosTests
{
    private static Domain.Entities.OrdemServico Recebida() => new(1, 1);

    private static Domain.Entities.OrdemServico EmDiagnostico()
    {
        var ordem = Recebida();
        ordem.IniciarDiagnostico();
        return ordem;
    }

    private static Domain.Entities.OrdemServico ComOrcamento()
    {
        var ordem = EmDiagnostico();
        ordem.AdicionarServico(1, "Revisão", 100m);
        ordem.ConcluirDiagnostico();
        ordem.GerarOrcamento();
        return ordem;
    }

    private static Domain.Entities.OrdemServico AguardandoAprovacao()
    {
        var ordem = ComOrcamento();
        ordem.DisponibilizarOrcamento();
        return ordem;
    }

    private static Domain.Entities.OrdemServico EmExecucao()
    {
        var ordem = AguardandoAprovacao();
        ordem.AprovarOrcamento();
        ordem.IniciarExecucao();
        return ordem;
    }

    private static void AssertStatusInvalido(Action acao, EnumsOrdemServico.StatusOrdemServico status)
    {
        var ex = Assert.Throws<InvalidOperationException>(acao);
        Assert.Equal($"Operação inválida para uma Ordem de Serviço com status '{status}'.", ex.Message);
    }

    #region Status incorreto
    [Fact]
    public void GerarOrcamento_DeveLancarExcecao_QuandoOrdemNaoEstaEmDiagnostico()
    {
        var ordem = Recebida();

        AssertStatusInvalido(ordem.GerarOrcamento, EnumsOrdemServico.StatusOrdemServico.Recebida);
        Assert.Null(ordem.Orcamento);
    }

    [Fact]
    public void GerarOrcamento_DeveLancarExcecao_QuandoOrdemJaAguardaAprovacao()
    {
        var ordem = AguardandoAprovacao();

        AssertStatusInvalido(ordem.GerarOrcamento, EnumsOrdemServico.StatusOrdemServico.AguardandoAprovacao);
    }

    [Fact]
    public void DisponibilizarOrcamento_DeveLancarExcecao_QuandoOrdemNaoEstaEmDiagnostico()
    {
        var ordem = Recebida();

        AssertStatusInvalido(ordem.DisponibilizarOrcamento, EnumsOrdemServico.StatusOrdemServico.Recebida);
    }

    [Fact]
    public void DisponibilizarOrcamento_DeveLancarExcecao_QuandoOrcamentoJaFoiDisponibilizado()
    {
        var ordem = AguardandoAprovacao();

        AssertStatusInvalido(ordem.DisponibilizarOrcamento, EnumsOrdemServico.StatusOrdemServico.AguardandoAprovacao);
    }

    [Fact]
    public void DisponibilizarOrcamento_DeveLancarExcecao_QuandoOrcamentoNaoFoiGerado()
    {
        var ordem = EmDiagnostico();

        var ex = Assert.Throws<InvalidOperationException>(ordem.DisponibilizarOrcamento);

        Assert.Equal("O orçamento ainda não foi gerado.", ex.Message);
        Assert.Equal(EnumsOrdemServico.StatusOrdemServico.EmDiagnostico, ordem.Status);
    }

    [Fact]
    public void AprovarOrcamento_DeveLancarExcecao_QuandoOrdemNaoAguardaAprovacao()
    {
        var ordem = ComOrcamento();

        AssertStatusInvalido(ordem.AprovarOrcamento, EnumsOrdemServico.StatusOrdemServico.EmDiagnostico);
        Assert.Equal(EnumsOrcamento.StatusOrcamento.Pendente, ordem.Orcamento!.Status);
    }

    [Fact]
    public void RejeitarOrcamento_DeveLancarExcecao_QuandoOrdemNaoAguardaAprovacao()
    {
        var ordem = ComOrcamento();

        AssertStatusInvalido(ordem.RejeitarOrcamento, EnumsOrdemServico.StatusOrdemServico.EmDiagnostico);
        Assert.Equal(EnumsOrcamento.StatusOrcamento.Pendente, ordem.Orcamento!.Status);
    }

    [Fact]
    public void AprovarOrcamento_DeveLancarExcecao_QuandoOrcamentoJaFoiDecidido()
    {
        var ordem = AguardandoAprovacao();
        ordem.AprovarOrcamento();

        var ex = Assert.Throws<InvalidOperationException>(ordem.AprovarOrcamento);

        Assert.Equal("Somente um orçamento pendente pode ser aprovado.", ex.Message);
    }

    [Fact]
    public void RejeitarOrcamento_DeveLancarExcecao_QuandoOrcamentoJaFoiDecidido()
    {
        var ordem = AguardandoAprovacao();
        ordem.RejeitarOrcamento();

        var ex = Assert.Throws<InvalidOperationException>(ordem.RejeitarOrcamento);

        Assert.Equal("Somente um orçamento pendente pode ser rejeitado.", ex.Message);
    }

    [Fact]
    public void IniciarExecucao_DeveLancarExcecao_QuandoOrcamentoNaoFoiAprovado()
    {
        var ordem = AguardandoAprovacao();

        var ex = Assert.Throws<InvalidOperationException>(ordem.IniciarExecucao);

        Assert.Equal("A execução somente pode ser iniciada após a aprovação do orçamento.", ex.Message);
        Assert.Equal(EnumsOrdemServico.StatusOrdemServico.AguardandoAprovacao, ordem.Status);
    }

    [Fact]
    public void IniciarExecucao_DeveLancarExcecao_QuandoOrcamentoFoiRejeitado()
    {
        var ordem = AguardandoAprovacao();
        ordem.RejeitarOrcamento();

        Assert.Throws<InvalidOperationException>(ordem.IniciarExecucao);
        Assert.Equal(EnumsOrdemServico.StatusOrdemServico.AguardandoAprovacao, ordem.Status);
        Assert.Null(ordem.DataInicioExecucao);
    }

    [Fact]
    public void IniciarExecucao_DeveLancarExcecao_QuandoOrdemNaoAguardaAprovacao()
    {
        var ordem = EmDiagnostico();

        AssertStatusInvalido(ordem.IniciarExecucao, EnumsOrdemServico.StatusOrdemServico.EmDiagnostico);
    }

    [Fact]
    public void Finalizar_DeveLancarExcecao_QuandoOrdemNaoEstaEmExecucao()
    {
        var ordem = AguardandoAprovacao();

        AssertStatusInvalido(ordem.Finalizar, EnumsOrdemServico.StatusOrdemServico.AguardandoAprovacao);
        Assert.Null(ordem.DataFinalizacao);
    }

    [Fact]
    public void Entregar_DeveLancarExcecao_QuandoOrdemNaoEstaFinalizada()
    {
        var ordem = EmExecucao();

        AssertStatusInvalido(ordem.Entregar, EnumsOrdemServico.StatusOrdemServico.EmExecucao);
        Assert.Null(ordem.DataEntrega);
    }

    [Fact]
    public void ConcluirDiagnostico_DeveLancarExcecao_QuandoDiagnosticoJaFoiConcluido()
    {
        var ordem = EmDiagnostico();
        ordem.ConcluirDiagnostico();

        var ex = Assert.Throws<InvalidOperationException>(ordem.ConcluirDiagnostico);

        Assert.Equal("O diagnóstico já foi concluído.", ex.Message);
    }

    [Fact]
    public void ConcluirDiagnostico_DeveLancarExcecao_QuandoOrdemNaoEstaEmDiagnostico()
    {
        var ordem = Recebida();

        AssertStatusInvalido(ordem.ConcluirDiagnostico, EnumsOrdemServico.StatusOrdemServico.Recebida);
        Assert.Null(ordem.DataConclusaoDiagnostico);
    }
    #endregion

    #region Mensagens das regras de orçamento
    [Fact]
    public void GerarOrcamento_DeveLancarExcecao_QuandoDiagnosticoNaoFoiConcluido()
    {
        var ordem = EmDiagnostico();
        ordem.AdicionarServico(1, "Revisão", 100m);

        var ex = Assert.Throws<InvalidOperationException>(ordem.GerarOrcamento);

        Assert.Equal("O diagnóstico deve ser concluído antes da geração do orçamento.", ex.Message);
    }

    [Fact]
    public void GerarOrcamento_DeveLancarExcecao_QuandoNaoHaServicos()
    {
        var ordem = EmDiagnostico();
        ordem.ConcluirDiagnostico();

        var ex = Assert.Throws<InvalidOperationException>(ordem.GerarOrcamento);

        Assert.Equal("A Ordem de Serviço deve possuir pelo menos um serviço.", ex.Message);
    }

    [Fact]
    public void GerarOrcamento_DeveLancarExcecao_QuandoOrcamentoJaExiste()
    {
        var ordem = ComOrcamento();

        // O status continua EmDiagnostico até o orçamento ser disponibilizado
        var ex = Assert.Throws<InvalidOperationException>(ordem.GerarOrcamento);

        Assert.Equal("Já existe um orçamento para esta Ordem de Serviço.", ex.Message);
    }

    [Fact]
    public void AdicionarServico_DeveLancarExcecao_AposAGeracaoDoOrcamento()
    {
        var ordem = ComOrcamento();

        var ex = Assert.Throws<InvalidOperationException>(() => ordem.AdicionarServico(2, "Outro", 10m));

        Assert.Equal("Não é possível adicionar serviços após a geração do orçamento.", ex.Message);
    }

    [Fact]
    public void AdicionarPecaInsumo_DeveLancarExcecao_AposAGeracaoDoOrcamento()
    {
        var ordem = ComOrcamento();

        var ex = Assert.Throws<InvalidOperationException>(
            () => ordem.AdicionarPecaInsumo(1, "Filtro", EnumsConsumivel.TipoConsumivel.Peca, 1, 10m));

        Assert.Equal("Não é possível adicionar peças ou insumos após a geração do orçamento.", ex.Message);
    }

    [Fact]
    public void AdicionarServico_DeveLancarExcecao_QuandoOrdemJaFoiFinalizada()
    {
        var ordem = EmExecucao();
        ordem.Finalizar();

        Assert.Throws<InvalidOperationException>(() => ordem.AdicionarServico(2, "Outro", 10m));
    }
    #endregion

    #region Fronteira de valor zero
    [Fact]
    public void AdicionarServico_DeveAceitarValorZero()
    {
        var ordem = EmDiagnostico();

        ordem.AdicionarServico(1, "Cortesia", 0m);

        var item = Assert.Single(ordem.ItensServico);
        Assert.Equal(0m, item.Valor);
    }

    [Fact]
    public void AdicionarServico_DeveLancarExcecao_QuandoValorNegativo()
    {
        var ordem = EmDiagnostico();

        Assert.Throws<ArgumentOutOfRangeException>(() => ordem.AdicionarServico(1, "Revisão", -0.01m));
    }

    [Fact]
    public void AdicionarPecaInsumo_DeveAceitarValorUnitarioZero()
    {
        var ordem = EmDiagnostico();

        ordem.AdicionarPecaInsumo(1, "Brinde", EnumsConsumivel.TipoConsumivel.Insumo, 1, 0m);

        var item = Assert.Single(ordem.ItensPecaInsumo);
        Assert.Equal(0m, item.ValorUnitario);
        Assert.Equal(0m, item.ValorTotal);
    }

    [Fact]
    public void AdicionarPecaInsumo_DeveLancarExcecao_QuandoValorUnitarioNegativo()
    {
        var ordem = EmDiagnostico();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ordem.AdicionarPecaInsumo(1, "Filtro", EnumsConsumivel.TipoConsumivel.Peca, 1, -0.01m));
    }

    [Fact]
    public void GerarOrcamento_DeveAceitarValorTotalZero()
    {
        var ordem = EmDiagnostico();
        ordem.AdicionarServico(1, "Cortesia", 0m);
        ordem.ConcluirDiagnostico();

        ordem.GerarOrcamento();

        Assert.Equal(0m, ordem.Orcamento!.ValorServicos);
        Assert.Equal(0m, ordem.Orcamento.ValorPecasInsumos);
        Assert.Equal(0m, ordem.Orcamento.ValorTotal);
        Assert.Equal(EnumsOrcamento.StatusOrcamento.Pendente, ordem.Orcamento.Status);
    }
    #endregion
}
