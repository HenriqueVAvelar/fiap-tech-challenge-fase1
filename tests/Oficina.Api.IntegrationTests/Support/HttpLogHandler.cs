using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Oficina.Api.IntegrationTests.Support;

/// <summary>
/// Imprime no console cada requisição e resposta HTTP dos testes (método, URI, cabeçalhos, corpo e status),
/// como o log do REST-assured. Fica desligado no CI (variável CI=true) ou com HTTP_LOG=0.
/// </summary>
public class HttpLogHandler : DelegatingHandler
{
    private static readonly JsonSerializerOptions Indentado = new() { WriteIndented = true };

    static HttpLogHandler()
    {
        // Evita acentos quebrados no console do Windows
        try { Console.OutputEncoding = Encoding.UTF8; } catch (IOException) { }
    }

    public static readonly bool Ativo =
        Environment.GetEnvironmentVariable("HTTP_LOG") != "0" &&
        Environment.GetEnvironmentVariable("CI") != "true";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!Ativo)
            return await base.SendAsync(request, cancellationToken);

        var log = new StringBuilder();
        log.AppendLine();
        log.AppendLine($"=== {request.Method} {request.RequestUri?.PathAndQuery}");

        AppendHeaders(log, "Request headers", request.Headers, request.Content?.Headers);

        if (request.Content is not null)
            log.AppendLine($"Request body:{Environment.NewLine}{Formatar(await request.Content.ReadAsStringAsync(cancellationToken))}");

        var resposta = await base.SendAsync(request, cancellationToken);

        log.AppendLine($"--- {(int)resposta.StatusCode} {resposta.StatusCode}");
        AppendHeaders(log, "Response headers", resposta.Headers, resposta.Content.Headers);

        // Guarda o conteúdo em memória para o teste ainda conseguir lê-lo depois
        await resposta.Content.LoadIntoBufferAsync();
        var corpo = await resposta.Content.ReadAsStringAsync(cancellationToken);

        if (corpo.Length > 0)
            log.AppendLine($"Response body:{Environment.NewLine}{Formatar(corpo)}");

        Console.Out.Write(log.ToString());

        return resposta;
    }

    private static void AppendHeaders(StringBuilder log, string titulo, params System.Net.Http.Headers.HttpHeaders?[] grupos)
    {
        var linhas = grupos
            .Where(g => g is not null)
            .SelectMany(g => g!)
            .Select(h => h.Key == "Authorization" ? "Authorization: Bearer ***" : $"{h.Key}: {string.Join(", ", h.Value)}")
            .ToList();

        if (linhas.Count > 0)
            log.AppendLine($"{titulo}: {string.Join(" | ", linhas)}");
    }

    private static string Formatar(string corpo)
    {
        // Senhas não vão para o log; o token é abreviado
        corpo = Regex.Replace(corpo, "(\"senha\"\\s*:\\s*\")[^\"]*(\")", "$1***$2", RegexOptions.IgnoreCase);
        corpo = Regex.Replace(corpo, "(\"token\"\\s*:\\s*\")([^\"]{12})[^\"]*(\")", "$1$2...$3", RegexOptions.IgnoreCase);

        try
        {
            using var documento = JsonDocument.Parse(corpo);

            return JsonSerializer.Serialize(documento.RootElement, Indentado);
        }
        catch (JsonException)
        {
            return corpo;
        }
    }
}
