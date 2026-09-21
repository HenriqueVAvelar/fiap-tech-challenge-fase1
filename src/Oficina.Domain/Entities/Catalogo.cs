using Oficina.Domain.Enums;

namespace Oficina.Domain.Entities;

public class Catalogo
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public decimal Preco { get; private set; }
    public EnumsConsumivel.TipoConsumivel Tipo { get; private set; }
    public bool Ativo { get; private set; }
    public string? Descricao { get; private set; }

    private Catalogo()
    {
        Nome = string.Empty;
    }

    public Catalogo(string nome, decimal preco, EnumsConsumivel.TipoConsumivel tipo, bool ativo, string? descricao = null)
    {
        ValidarInvariantes(nome, preco);

        Nome = nome;
        Preco = preco;
        Tipo = tipo;
        Ativo = ativo;
        Descricao = descricao;
    }

    public void Atualizar(string nome, decimal preco, EnumsConsumivel.TipoConsumivel tipo, bool ativo, string? descricao)
    {
        ValidarInvariantes(nome, preco);

        Nome = nome;
        Preco = preco;
        Tipo = tipo;
        Ativo = ativo;
        Descricao = descricao;
    }

    private static void ValidarInvariantes(string nome, decimal preco)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do item de catálogo é obrigatório.", nameof(nome));

        if (preco < 0)
            throw new ArgumentException("O preço do item de catálogo não pode ser negativo.", nameof(preco));
    }
}
