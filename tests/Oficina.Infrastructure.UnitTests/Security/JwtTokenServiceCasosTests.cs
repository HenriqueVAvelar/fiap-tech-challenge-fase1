using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Oficina.Domain.Entities;
using Oficina.Domain.Enums;
using Oficina.Infrastructure.Security;

namespace Oficina.Infrastructure.UnitTests.Security;

public class JwtTokenServiceCasosTests
{
    private static JwtSettings Configuracao(int expiracaoMinutos = 60) => new()
    {
        Secret = "chave-de-teste-com-mais-de-trinta-e-dois-caracteres",
        Issuer = "Oficina.Api",
        Audience = "Oficina.Api",
        ExpirationMinutes = expiracaoMinutos
    };

    private static JwtTokenService CriarService(JwtSettings settings) => new(Options.Create(settings));

    private static Usuario CriarUsuario(EnumsUsuario.PerfilUsuario perfil = EnumsUsuario.PerfilUsuario.Mecanico)
    {
        var usuario = new Usuario("Joao", "joao", "hash", perfil);
        typeof(Usuario).GetProperty(nameof(Usuario.Id))!.SetValue(usuario, 42);
        return usuario;
    }

    [Fact]
    public void GerarToken_DeveConterOIdDoUsuarioNoSubject()
    {
        var resultado = CriarService(Configuracao()).GerarToken(CriarUsuario());

        var token = new JsonWebTokenHandler().ReadJsonWebToken(resultado.Token);

        Assert.Equal("42", token.GetClaim(JwtRegisteredClaimNames.Sub).Value);
    }

    [Theory]
    [InlineData(EnumsUsuario.PerfilUsuario.Administrador, "Administrador")]
    [InlineData(EnumsUsuario.PerfilUsuario.Mecanico, "Mecanico")]
    public void GerarToken_DeveConterOPerfilComoRole(EnumsUsuario.PerfilUsuario perfil, string esperado)
    {
        var resultado = CriarService(Configuracao()).GerarToken(CriarUsuario(perfil));

        var token = new JsonWebTokenHandler().ReadJsonWebToken(resultado.Token);

        Assert.Equal(esperado, token.GetClaim(JwtTokenService.RoleClaim).Value);
    }

    [Fact]
    public void GerarToken_DeveContarAAudiencia()
    {
        var resultado = CriarService(Configuracao()).GerarToken(CriarUsuario());

        var token = new JsonWebTokenHandler().ReadJsonWebToken(resultado.Token);

        Assert.Contains("Oficina.Api", token.Audiences);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(120)]
    public void GerarToken_DeveRespeitarOTempoDeExpiracaoConfigurado(int minutos)
    {
        var antes = DateTime.UtcNow;

        var resultado = CriarService(Configuracao(minutos)).GerarToken(CriarUsuario());

        Assert.InRange(resultado.ExpiraEm, antes.AddMinutes(minutos - 1), antes.AddMinutes(minutos + 1));
    }

    [Fact]
    public async Task GerarToken_NaoDeveSerValido_ComOutraChave()
    {
        var resultado = CriarService(Configuracao()).GerarToken(CriarUsuario());

        var validacao = await new JsonWebTokenHandler().ValidateTokenAsync(resultado.Token, new TokenValidationParameters
        {
            ValidIssuer = "Oficina.Api",
            ValidAudience = "Oficina.Api",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("outra-chave-de-teste-com-mais-de-trinta-e-dois-caracteres"))
        });

        Assert.False(validacao.IsValid);
    }

    [Fact]
    public async Task GerarToken_NaoDeveSerValido_ParaOutroEmissor()
    {
        var resultado = CriarService(Configuracao()).GerarToken(CriarUsuario());

        var validacao = await new JsonWebTokenHandler().ValidateTokenAsync(resultado.Token, new TokenValidationParameters
        {
            ValidIssuer = "Outro.Emissor",
            ValidAudience = "Oficina.Api",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuracao().Secret))
        });

        Assert.False(validacao.IsValid);
    }

    [Fact]
    public void GerarToken_DeveGerarTokensDiferentes_ParaUsuariosDiferentes()
    {
        var service = CriarService(Configuracao());

        var maria = new Usuario("Maria", "maria", "hash", EnumsUsuario.PerfilUsuario.Administrador);

        Assert.NotEqual(
            service.GerarToken(CriarUsuario()).Token,
            service.GerarToken(maria).Token);
    }
}
