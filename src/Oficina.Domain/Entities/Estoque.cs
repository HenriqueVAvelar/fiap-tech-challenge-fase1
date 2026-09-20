namespace Oficina.Domain.Entities;

public class Estoque
{
    public Guid Id { get; set; }
    public Guid CatalogoId { get; set; }
    public int Quantidade { get; set; }
    public Catalogo Catalogo { get; set; }


    public Estoque(int quantidadeInicial = 0)
    {
        if (quantidadeInicial < 0)
            throw new ArgumentOutOfRangeException(nameof(quantidadeInicial),"A quantidade inicial não pode ser negativa.");

        Id = Guid.NewGuid();
        Quantidade = quantidadeInicial;
    }

    public void RegistrarEntrada(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade),"A quantidade da entrada deve ser maior que zero.");

        Quantidade += quantidade;
    }

    public void Consumir(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade),"A quantidade consumida deve ser maior que zero.");

        if (quantidade > Quantidade)
            throw new InvalidOperationException($"Não é possível consumir {quantidade}. Quantidade em estoque: {Quantidade}.");

        Quantidade -= quantidade;
    }
}
