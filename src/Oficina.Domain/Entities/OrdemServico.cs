using Oficina.Domain.Enums;

namespace Oficina.Domain.Entities;

public class OrdemServico
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int VeiculoId { get; set; }
    public EnumsOrdemServico.StatusOrdemServico Status { get; set; }
    public DateTime DataCriacao { get; set; }
    public string ItensServico { get; set; }
    public string ItensPecaInsumo { get; set; }
    public decimal Orcamento { get; set; }

    public Cliente Cliente { get; set; }
    public Veiculo Veiculo { get; set; }
}
