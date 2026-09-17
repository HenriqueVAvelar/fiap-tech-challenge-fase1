namespace Oficina.Domain.Entities;

public class Servico
{
    public int Id { get; set; }
    public string Descricao{ get; set; }
    public decimal Valor { get; set; }
    public bool Ativo { get; set; }
}
