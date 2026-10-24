using Oficina.Infrastructure.Security;

namespace Oficina.Infrastructure.UnitTests.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_NaoDeveExporASenhaEmTextoPlano()
    {
        var hash = _hasher.Hash("Senha@123");

        Assert.NotEqual("Senha@123", hash);
        Assert.DoesNotContain("Senha@123", hash);
    }

    [Fact]
    public void Hash_DeveGerarValoresDiferentes_ParaAMesmaSenha()
    {
        Assert.NotEqual(_hasher.Hash("Senha@123"), _hasher.Hash("Senha@123"));
    }

    [Fact]
    public void Verificar_DeveRetornarTrue_ParaASenhaCorreta()
    {
        var hash = _hasher.Hash("Senha@123");

        Assert.True(_hasher.Verificar(hash, "Senha@123"));
    }

    [Fact]
    public void Verificar_DeveRetornarFalse_ParaSenhaIncorreta()
    {
        var hash = _hasher.Hash("Senha@123");

        Assert.False(_hasher.Verificar(hash, "senha@123"));
    }
}
