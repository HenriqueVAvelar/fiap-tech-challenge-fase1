using System.Text.Json;
using System.Text.Json.Serialization;

namespace Oficina.Api.IntegrationTests.Support;

public static class Json
{
    // Mesmas opções da API: camelCase e enums como texto
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
