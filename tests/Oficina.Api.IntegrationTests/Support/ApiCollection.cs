namespace Oficina.Api.IntegrationTests.Support;

/// <summary>Um único container PostgreSQL e uma única API para todas as classes de teste.</summary>
[CollectionDefinition(Nome)]
[LogNomeDoTeste]
public class ApiCollection : ICollectionFixture<OficinaApiFactory>
{
    public const string Nome = "Api";
}
