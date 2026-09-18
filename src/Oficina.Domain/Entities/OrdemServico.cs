using Oficina.Domain.Enums;

namespace Oficina.Domain.Entities;

public class OrdemServico
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public EnumsOrdemServico.StatusOrdemServico Status { get; set; }
    public DateTime DataCriacao { get; set; }
    public string ItensServico { get; set; }
    public string ItensPecaInsumo { get; set; }
    public decimal Orcamento { get; set; }

    public Cliente Cliente { get; set; }
    public Veiculo Veiculo { get; set; }
}
