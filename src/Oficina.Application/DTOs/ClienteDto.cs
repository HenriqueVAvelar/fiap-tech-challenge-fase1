using Oficina.Domain.Enums;

namespace Oficina.Application.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public required string Nome { get; set; }

    /// <summary>Somente os dígitos do CPF/CNPJ.</summary>
    public required string Documento { get; set; }

    public EnumsDocumento.TipoDocumento TipoDocumento { get; set; }
}
