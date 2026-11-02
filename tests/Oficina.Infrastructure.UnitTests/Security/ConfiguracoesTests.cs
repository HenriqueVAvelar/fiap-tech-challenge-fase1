using Oficina.Infrastructure.Security;
using Oficina.Infrastructure.Seed;

namespace Oficina.Infrastructure.UnitTests.Security;

public class ConfiguracoesTests
{
    [Fact]
    public void JwtSettings_DeveTerValoresPadrao()
    {
        var settings = new JwtSettings();

        Assert.Equal("Jwt", JwtSettings.SectionName);
        Assert.Equal(string.Empty, settings.Secret);
        Assert.Equal(string.Empty, settings.Issuer);
        Assert.Equal(string.Empty, settings.Audience);
        Assert.Equal(60, settings.ExpirationMinutes);
    }

    [Fact]
    public void AdminSeedSettings_DeveTerValoresPadrao()
    {
        var settings = new AdminSeedSettings();

        Assert.Equal("AdminSeed", AdminSeedSettings.SectionName);
        Assert.Equal(string.Empty, settings.Nome);
        Assert.Equal(string.Empty, settings.UserName);
        Assert.Equal(string.Empty, settings.Senha);
    }

    [Fact]
    public void DemoDataSettings_DeveIniciarDesabilitado()
    {
        var settings = new DemoDataSettings();

        Assert.Equal("DemoData", DemoDataSettings.SectionName);
        Assert.False(settings.Habilitado);
    }
}
