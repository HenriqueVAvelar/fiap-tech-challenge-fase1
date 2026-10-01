namespace Oficina.Domain.Entities;

public class ItemServico
{
    public int Id { get; private set; }
    public int OrdemServicoId { get; private set; }
    public int ServicoId { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }

    private ItemServico()
    {
    }

    internal ItemServico(int servicoId, string descricao, decimal valor)
    {
        if (servicoId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(servicoId),
                "O serviço informado é inválido.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException(
                "A descrição do serviço é obrigatória.",
                nameof(descricao));

        if (valor < 0)
            throw new ArgumentOutOfRangeException(
                nameof(valor),
                "O valor do serviço não pode ser negativo.");

        ServicoId = servicoId;
        Descricao = descricao;
        Valor = valor;
    }
}