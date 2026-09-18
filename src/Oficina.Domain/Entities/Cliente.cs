using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Entities;

public class Cliente
{
    public Guid Id { get; set; }
    public string Nome { get; set; }

    public Documento Documento { get; private set; }

    public Cliente(string nome, Documento documento)
    {
        Nome = nome;
        Documento = documento ?? throw new ArgumentNullException(nameof(documento));
    }
}
