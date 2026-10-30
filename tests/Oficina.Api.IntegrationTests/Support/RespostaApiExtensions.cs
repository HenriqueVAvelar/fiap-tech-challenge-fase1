using System.Net;
using System.Net.Http.Json;
using Xunit.Sdk;

namespace Oficina.Api.IntegrationTests.Support;

public static class RespostaApiExtensions
{
    /// <summary>
    /// Valida o status HTTP. Quando difere do esperado, a falha traz a requisição e a resposta completas,
    /// para que o log só apareça quando o teste falha.
    /// </summary>
    public static async Task<HttpResponseMessage> DeveTerStatusAsync(this HttpResponseMessage resposta, HttpStatusCode esperado)
    {
        if (resposta.StatusCode == esperado)
            return resposta;

        var requisicao = resposta.RequestMessage;
        var corpo = await resposta.Content.ReadAsStringAsync();

        throw new XunitException(
            $"Status esperado: {(int)esperado} {esperado}; recebido: {(int)resposta.StatusCode} {resposta.StatusCode}." +
            $"{Environment.NewLine}Requisição: {requisicao?.Method} {requisicao?.RequestUri}" +
            $"{Environment.NewLine}Content-Type: {resposta.Content.Headers.ContentType}" +
            $"{Environment.NewLine}Corpo: {corpo}");
    }

    public static HttpResponseMessage DeveSerJson(this HttpResponseMessage resposta)
    {
        Assert.Equal("application/json", resposta.Content.Headers.ContentType?.MediaType);

        return resposta;
    }

    public static async Task<T> LerAsync<T>(this HttpResponseMessage resposta)
        => await resposta.Content.ReadFromJsonAsync<T>(Json.Options)
           ?? throw new XunitException($"A resposta não trouxe um {typeof(T).Name} no corpo.");

    public static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string uri, T corpo)
        => client.PostAsJsonAsync(uri, corpo, Json.Options);

    public static Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient client, string uri, T corpo)
        => client.PutAsJsonAsync(uri, corpo, Json.Options);
}
