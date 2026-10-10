using Oficina.Domain.Enums;

namespace Oficina.Domain.Entities;

public class Orcamento
{
    public int Id { get; private set; }
    public int OrdemServicoId { get; private set; }

    public decimal ValorServicos { get; private set; }
    public decimal ValorPecasInsumos { get; private set; }
    public decimal ValorTotal { get; private set; }

    public EnumsOrcamento.StatusOrcamento Status { get; private set; }

    public DateTime DataGeracao { get; private set; }
    public DateTime? DataDisponibilizacao { get; private set; }
    public DateTime? DataDecisao { get; private set; }

    private Orcamento()
    {
    }

    internal Orcamento(
        decimal valorServicos,
        decimal valorPecasInsumos)
    {
        if (valorServicos < 0)
            throw new ArgumentOutOfRangeException(nameof(valorServicos));

        if (valorPecasInsumos < 0)
            throw new ArgumentOutOfRangeException(nameof(valorPecasInsumos));

        ValorServicos = valorServicos;
        ValorPecasInsumos = valorPecasInsumos;
        ValorTotal = valorServicos + valorPecasInsumos;

        Status = EnumsOrcamento.StatusOrcamento.Pendente;
        DataGeracao = DateTime.UtcNow;
    }

    internal void Disponibilizar()
    {
        if (DataDisponibilizacao.HasValue)
            throw new InvalidOperationException(
                "O orçamento já foi disponibilizado.");

        DataDisponibilizacao = DateTime.UtcNow;
    }

    internal void Aprovar()
    {
        if (Status != EnumsOrcamento.StatusOrcamento.Pendente)
            throw new InvalidOperationException(
                "Somente um orçamento pendente pode ser aprovado.");

        Status = EnumsOrcamento.StatusOrcamento.Aprovado;
        DataDecisao = DateTime.UtcNow;
    }

    internal void Rejeitar()
    {
        if (Status != EnumsOrcamento.StatusOrcamento.Pendente)
            throw new InvalidOperationException(
                "Somente um orçamento pendente pode ser rejeitado.");

        Status = EnumsOrcamento.StatusOrcamento.Rejeitado;
        DataDecisao = DateTime.UtcNow;
    }
}