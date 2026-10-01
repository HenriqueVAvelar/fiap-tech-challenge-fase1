using Oficina.Domain.Enums;

namespace Oficina.Domain.Entities;

public class ItemPecaInsumo
{
    public int Id { get; private set; }
    public int OrdemServicoId { get; private set; }
    public int CatalogoId { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public EnumsConsumivel.TipoConsumivel Tipo { get; private set; }

    public int Quantidade { get; private set; }

    public decimal ValorUnitario { get; private set; }

    public decimal ValorTotal { get; private set; }

    private ItemPecaInsumo()
    {
    }

    internal ItemPecaInsumo(
        int catalogoId,
        string nome,
        EnumsConsumivel.TipoConsumivel tipo,
        int quantidade,
        decimal valorUnitario)
    {
        if (catalogoId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(catalogoId),
                "O item de catálogo informado é inválido.");

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException(
                "O nome da peça ou insumo é obrigatório.",
                nameof(nome));

        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantidade),
                "A quantidade deve ser maior que zero.");

        if (valorUnitario < 0)
            throw new ArgumentOutOfRangeException(
                nameof(valorUnitario),
                "O valor unitário não pode ser negativo.");

        CatalogoId = catalogoId;
        Nome = nome;
        Tipo = tipo;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
        ValorTotal = quantidade * valorUnitario;
    }
}