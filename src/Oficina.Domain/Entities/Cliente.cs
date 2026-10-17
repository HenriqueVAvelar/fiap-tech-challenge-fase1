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
        ValidarNome(nome);

        Nome = nome.Trim();
        Documento = documento ?? throw new ArgumentNullException(nameof(documento));
    }

    public void Atualizar(string nome, Documento documento)
    {
        ValidarNome(nome);

        Nome = nome.Trim();
        Documento = documento ?? throw new ArgumentNullException(nameof(documento));
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do cliente é obrigatório.", nameof(nome));
    }
}
