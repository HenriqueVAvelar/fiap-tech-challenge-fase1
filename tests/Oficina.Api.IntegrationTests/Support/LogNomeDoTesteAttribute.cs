using System.Reflection;
using Xunit.Sdk;

namespace Oficina.Api.IntegrationTests.Support;

/// <summary>
/// Escreve o nome do teste antes das requisições dele, para o log HTTP mostrar a qual teste cada chamada pertence.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
public class LogNomeDoTesteAttribute : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest)
    {
        if (!HttpLogHandler.Ativo)
            return;

        Console.Out.WriteLine();
        Console.Out.WriteLine($"##### TESTE: {methodUnderTest.DeclaringType?.Name}.{methodUnderTest.Name}");
    }
}
