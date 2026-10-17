namespace Oficina.Application.DTOs;

public class ServicoDto
{
    public int Id { get; set; }
    public required string Descricao { get; set; }
    public decimal Valor { get; set; }
    public bool Ativo { get; set; }
}
