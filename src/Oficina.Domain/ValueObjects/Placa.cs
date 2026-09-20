using System.Text.RegularExpressions;

namespace Oficina.Domain.ValueObjects;

public class Placa
{
    private static readonly Regex FormatoAntigo =
        new("^[A-Z]{3}[0-9]{4}$", RegexOptions.Compiled);

    private static readonly Regex FormatoMercosul =
        new("^[A-Z]{3}[0-9][A-Z][0-9]{2}$", RegexOptions.Compiled);

    public string Valor { get; }

    public Placa(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("A placa não pode ser vazia.", nameof(valor));
        }

        var normalizada = Normalizar(valor);

        if (!FormatoAntigo.IsMatch(normalizada) && !FormatoMercosul.IsMatch(normalizada))
        {
            throw new ArgumentException(
                $"A placa '{valor}' não está em um formato válido.",
                nameof(valor));
        }

        Valor = normalizada;
    }

    private static string Normalizar(string valor)
    {
        var caracteres = valor.Where(char.IsLetterOrDigit).ToArray();

        return new string(caracteres).ToUpperInvariant();
    }

    public override string ToString() => Valor;
}
