namespace Oficina.Api.IntegrationTests.Support;

/// <summary>Gera documentos e placas válidos e únicos, para que os testes não colidam entre si no mesmo banco.</summary>
public static class DadosDeTeste
{
    private static readonly int[] PesosCnpjPrimeiroDigito = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
    private static readonly int[] PesosCnpjSegundoDigito = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

    public static string Cpf()
    {
        while (true)
        {
            var digitos = Enumerable.Range(0, 9).Select(_ => Random.Shared.Next(10)).ToList();

            if (digitos.Distinct().Count() == 1)
                continue;

            digitos.Add(DigitoCpf(digitos));
            digitos.Add(DigitoCpf(digitos));

            return string.Concat(digitos);
        }
    }

    public static string Cnpj()
    {
        var digitos = Enumerable.Range(0, 12).Select(_ => Random.Shared.Next(10)).ToList();

        digitos.Add(DigitoCnpj(digitos, PesosCnpjPrimeiroDigito));
        digitos.Add(DigitoCnpj(digitos, PesosCnpjSegundoDigito));

        return string.Concat(digitos);
    }

    public static string FormatarCpf(string cpf) => $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}";

    /// <summary>Placa no padrão Mercosul (ABC1D23).</summary>
    public static string Placa()
    {
        const string letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        char Letra() => letras[Random.Shared.Next(letras.Length)];

        return $"{Letra()}{Letra()}{Letra()}{Random.Shared.Next(10)}{Letra()}{Random.Shared.Next(10)}{Random.Shared.Next(10)}";
    }

    public static string Texto(string prefixo) => $"{prefixo}-{Guid.NewGuid():N}"[..(prefixo.Length + 9)];

    private static int DigitoCpf(List<int> digitos)
    {
        var soma = 0;
        for (var i = 0; i < digitos.Count; i++)
            soma += digitos[i] * (digitos.Count + 1 - i);

        var resto = soma * 10 % 11;
        return resto == 10 ? 0 : resto;
    }

    private static int DigitoCnpj(List<int> digitos, int[] pesos)
    {
        var soma = 0;
        for (var i = 0; i < pesos.Length; i++)
            soma += digitos[i] * pesos[i];

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
