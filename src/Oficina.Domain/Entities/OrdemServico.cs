using Oficina.Domain.Enums;

namespace Oficina.Domain.Entities;

public class OrdemServico
{
    private readonly List<ItemServico> _itensServico = new();
    private readonly List<ItemPecaInsumo> _itensPecaInsumo = new();

    public int Id { get; private set; }

    public int ClienteId { get; private set; }

    public int VeiculoId { get; private set; }

    public EnumsOrdemServico.StatusOrdemServico Status { get; private set; }

    public DateTime DataCriacao { get; private set; }

    public DateTime? DataInicioDiagnostico { get; private set; }

    public DateTime? DataConclusaoDiagnostico { get; private set; }

    public DateTime? DataInicioExecucao { get; private set; }

    public DateTime? DataFinalizacao { get; private set; }

    public DateTime? DataEntrega { get; private set; }

    public IReadOnlyCollection<ItemServico> ItensServico =>
        _itensServico.AsReadOnly();

    public IReadOnlyCollection<ItemPecaInsumo> ItensPecaInsumo =>
        _itensPecaInsumo.AsReadOnly();

    public Orcamento? Orcamento { get; private set; }

    private OrdemServico()
    {
    }

    public OrdemServico(int clienteId, int veiculoId)
    {
        if (clienteId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(clienteId),
                "O cliente informado é inválido.");

        if (veiculoId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(veiculoId),
                "O veículo informado é inválido.");

        ClienteId = clienteId;
        VeiculoId = veiculoId;

        Status = EnumsOrdemServico.StatusOrdemServico.Recebida;
        DataCriacao = DateTime.UtcNow;
    }

    public void AdicionarServico(
        int servicoId,
        string descricao,
        decimal valor)
    {
        if (Orcamento is not null)
            throw new InvalidOperationException(
                "Não é possível adicionar serviços após a geração do orçamento.");

        if (Status != EnumsOrdemServico.StatusOrdemServico.Recebida &&
            Status != EnumsOrdemServico.StatusOrdemServico.EmDiagnostico)
        {
            throw new InvalidOperationException(
                "Não é possível adicionar serviços no estado atual da Ordem de Serviço.");
        }

        _itensServico.Add(
            new ItemServico(servicoId, descricao, valor));
    }

    public void IniciarDiagnostico()
    {
        GarantirStatus(
            EnumsOrdemServico.StatusOrdemServico.Recebida);

        Status = EnumsOrdemServico.StatusOrdemServico.EmDiagnostico;
        DataInicioDiagnostico = DateTime.UtcNow;
    }

    public void AdicionarPecaInsumo(
        int catalogoId,
        string nome,
        EnumsConsumivel.TipoConsumivel tipo,
        int quantidade,
        decimal valorUnitario)
    {
        GarantirStatus(
            EnumsOrdemServico.StatusOrdemServico.EmDiagnostico);

        if (Orcamento is not null)
            throw new InvalidOperationException(
                "Não é possível adicionar peças ou insumos após a geração do orçamento.");

        _itensPecaInsumo.Add(
            new ItemPecaInsumo(
                catalogoId,
                nome,
                tipo,
                quantidade,
                valorUnitario));
    }

    public void ConcluirDiagnostico()
    {
        GarantirStatus(
            EnumsOrdemServico.StatusOrdemServico.EmDiagnostico);

        if (DataConclusaoDiagnostico.HasValue)
            throw new InvalidOperationException(
                "O diagnóstico já foi concluído.");

        DataConclusaoDiagnostico = DateTime.UtcNow;
    }

    public void GerarOrcamento()
    {
        GarantirStatus(
            EnumsOrdemServico.StatusOrdemServico.EmDiagnostico);

        if (!DataConclusaoDiagnostico.HasValue)
            throw new InvalidOperationException(
                "O diagnóstico deve ser concluído antes da geração do orçamento.");

        if (Orcamento is not null)
            throw new InvalidOperationException(
                "Já existe um orçamento para esta Ordem de Serviço.");

        if (_itensServico.Count == 0)
            throw new InvalidOperationException(
                "A Ordem de Serviço deve possuir pelo menos um serviço.");

        var valorServicos =
            _itensServico.Sum(item => item.Valor);

        var valorPecasInsumos =
            _itensPecaInsumo.Sum(item => item.ValorTotal);

        Orcamento = new Orcamento(
            valorServicos,
            valorPecasInsumos);
    }

    public void DisponibilizarOrcamento()
    {
        GarantirStatus(
            EnumsOrdemServico.StatusOrdemServico.EmDiagnostico);

        if (Orcamento is null)
            throw new InvalidOperationException(
                "O orçamento ainda não foi gerado.");

        Orcamento.Disponibilizar();

        Status =
            EnumsOrdemServico.StatusOrdemServico.AguardandoAprovacao;
    }

    public void AprovarOrcamento()
    {
        GarantirStatus(
            EnumsOrdemServico.StatusOrdemServico.AguardandoAprovacao);

        if (Orcamento is null)
            throw new InvalidOperationException(
                "O orçamento ainda não foi gerado.");

        Orcamento.Aprovar();
    }

    public void RejeitarOrcamento()
    {
        GarantirStatus(
            EnumsOrdemServico.StatusOrdemServico.AguardandoAprovacao);

        if (Orcamento is null)
            throw new InvalidOperationException(
                "O orçamento ainda não foi gerado.");

        Orcamento.Rejeitar();
    }

    public void IniciarExecucao()
    {
        GarantirStatus(
            EnumsOrdemServico.StatusOrdemServico.AguardandoAprovacao);

        if (Orcamento is null ||
            Orcamento.Status != EnumsOrcamento.StatusOrcamento.Aprovado)
        {
            throw new InvalidOperationException(
                "A execução somente pode ser iniciada após a aprovação do orçamento.");
        }

        Status = EnumsOrdemServico.StatusOrdemServico.EmExecucao;
        DataInicioExecucao = DateTime.UtcNow;
    }

    public void Finalizar()
    {
        GarantirStatus(
            EnumsOrdemServico.StatusOrdemServico.EmExecucao);

        Status = EnumsOrdemServico.StatusOrdemServico.Finalizada;
        DataFinalizacao = DateTime.UtcNow;
    }

    public void Entregar()
    {
        GarantirStatus(
            EnumsOrdemServico.StatusOrdemServico.Finalizada);

        Status = EnumsOrdemServico.StatusOrdemServico.Entregue;
        DataEntrega = DateTime.UtcNow;
    }

    private void GarantirStatus(
        EnumsOrdemServico.StatusOrdemServico statusEsperado)
    {
        if (Status != statusEsperado)
        {
            throw new InvalidOperationException(
                $"Operação inválida para uma Ordem de Serviço com status '{Status}'.");
        }
    }
}