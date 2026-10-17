namespace Oficina.Application.DTOs;

public class VeiculoDto
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public required string Placa { get; set; }
    public required string Marca { get; set; }
    public required string Modelo { get; set; }
    public int Ano { get; set; }
}
