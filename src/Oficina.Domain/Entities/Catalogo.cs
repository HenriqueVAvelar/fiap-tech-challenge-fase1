using Oficina.Domain.Enums;

namespace Oficina.Domain.Entities;

public class Catalogo
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public decimal Preco { get; set; }
    public EnumsConsumivel.TipoConsumivel Tipo { get; set; }
    public bool Ativo { get; set; }
    public string? Descricao { get; set; }    

    public Catalogo(string nome, decimal preco, EnumsConsumivel.TipoConsumivel tipo, bool ativo, string? descricao = null)
    {
        Nome = nome;
        Preco = preco;
        Tipo = tipo;
        Ativo = ativo;
        Descricao = descricao;
    }
}
