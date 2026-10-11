using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; }

    public Documento Documento { get; private set; }
        
    private Cliente()
    {
        Nome = string.Empty;
        Documento = null!;
    }

    public Cliente(string nome, Documento documento)
    {
        Nome = nome;
        Documento = documento ?? throw new ArgumentNullException(nameof(documento));
    }
}
