using Oficina.Domain.Enums;

namespace Oficina.Domain.ValueObjects;

/// <summary>
/// Documento de identificação do cliente (CPF ou CNPJ numérico).
/// Aceita a entrada com ou sem máscara, mas guarda somente os dígitos.
/// O tipo é deduzido pela quantidade de dígitos: 11 = CPF, 14 = CNPJ.
/// </summary>
public class Documento : IEquatable<Documento>
{
    private static readonly int[] PesosCnpjPrimeiroDigito = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
    private static readonly int[] PesosCnpjSegundoDigito = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

    public string Numero { get; private set; }

    public EnumsDocumento.TipoDocumento Tipo { get; private set; }

    // Usado pelo EF Core
    private Documento()
    {
        Numero = string.Empty;
    }

    public Documento(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("O documento não pode ser vazio.", nameof(valor));

        var numero = new string(valor.Where(char.IsDigit).ToArray());

        Tipo = numero.Length switch
        {
            11 => EnumsDocumento.TipoDocumento.Cpf,
            14 => EnumsDocumento.TipoDocumento.Cnpj,
            _ => throw new ArgumentException(
                "O documento deve ter 11 dígitos (CPF) ou 14 dígitos (CNPJ).",
                nameof(valor))
        };

        if (Tipo == EnumsDocumento.TipoDocumento.Cpf && !CpfValido(numero))
            throw new ArgumentException($"O CPF '{valor}' é inválido.", nameof(valor));

        if (Tipo == EnumsDocumento.TipoDocumento.Cnpj && !CnpjValido(numero))
            throw new ArgumentException($"O CNPJ '{valor}' é inválido.", nameof(valor));

        Numero = numero;
    }

    public string Formatado() => Tipo == EnumsDocumento.TipoDocumento.Cpf
        ? $"{Numero[..3]}.{Numero[3..6]}.{Numero[6..9]}-{Numero[9..]}"
        : $"{Numero[..2]}.{Numero[2..5]}.{Numero[5..8]}/{Numero[8..12]}-{Numero[12..]}";

    private static bool CpfValido(string numero)
    {
        if (TodosIguais(numero))
            return false;

        var primeiro = CalcularDigitoCpf(numero, 9);
        var segundo = CalcularDigitoCpf(numero, 10);

        return numero[9] - '0' == primeiro && numero[10] - '0' == segundo;
    }

    private static int CalcularDigitoCpf(string numero, int quantidade)
    {
        var soma = 0;
        for (var i = 0; i < quantidade; i++)
            soma += (numero[i] - '0') * (quantidade + 1 - i);

        var resto = soma * 10 % 11;
        return resto == 10 ? 0 : resto;
    }

    private static bool CnpjValido(string numero)
    {
        if (TodosIguais(numero))
            return false;

        var primeiro = CalcularDigitoCnpj(numero, PesosCnpjPrimeiroDigito);
        var segundo = CalcularDigitoCnpj(numero, PesosCnpjSegundoDigito);

        return numero[12] - '0' == primeiro && numero[13] - '0' == segundo;
    }

    private static int CalcularDigitoCnpj(string numero, int[] pesos)
    {
        var soma = 0;
        for (var i = 0; i < pesos.Length; i++)
            soma += (numero[i] - '0') * pesos[i];

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static bool TodosIguais(string numero) => numero.All(c => c == numero[0]);

    public bool Equals(Documento? other) => other is not null && Numero == other.Numero;

    public override bool Equals(object? obj) => Equals(obj as Documento);

    public override int GetHashCode() => Numero.GetHashCode();

    public override string ToString() => Numero;
}
