using Oficina.Infrastructure.Security;

namespace Oficina.Infrastructure.UnitTests.Security;

public class PasswordHasherCasosTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_DeveGerarUmHashNaoVazio()
    {
        Assert.False(string.IsNullOrWhiteSpace(_hasher.Hash("Senha@123")));
    }

    [Fact]
    public void Verificar_DeveAceitarCadaSenhaSomenteComSeuProprioHash()
    {
        var hashA = _hasher.Hash("Senha@123");
        var hashB = _hasher.Hash("Outra#456");

        Assert.True(_hasher.Verificar(hashA, "Senha@123"));
        Assert.True(_hasher.Verificar(hashB, "Outra#456"));
        Assert.False(_hasher.Verificar(hashA, "Outra#456"));
        Assert.False(_hasher.Verificar(hashB, "Senha@123"));
    }

    [Fact]
    public void Verificar_DeveDistinguirMaiusculasDeMinusculas()
    {
        var hash = _hasher.Hash("Senha@123");

        Assert.False(_hasher.Verificar(hash, "SENHA@123"));
    }

    [Fact]
    public void Verificar_DeveRetornarFalse_ParaSenhaVazia()
    {
        var hash = _hasher.Hash("Senha@123");

        Assert.False(_hasher.Verificar(hash, ""));
    }

    [Fact]
    public void Verificar_DeveFuncionarComHashsDeInstanciasDiferentes()
    {
        var hash = new PasswordHasher().Hash("Senha@123");

        Assert.True(new PasswordHasher().Verificar(hash, "Senha@123"));
    }

    [Fact]
    public void Verificar_DeveAceitarSenhaComCaracteresEspeciaisEAcentos()
    {
        var hash = _hasher.Hash("Çenha#ção!2026");

        Assert.True(_hasher.Verificar(hash, "Çenha#ção!2026"));
        Assert.False(_hasher.Verificar(hash, "Cenha#cao!2026"));
    }
}
