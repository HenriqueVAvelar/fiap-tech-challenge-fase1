using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Oficina.Domain.Entities;
using Oficina.Domain.Enums;
using Oficina.Infrastructure.Security;

namespace Oficina.Infrastructure.UnitTests.Security;

public class JwtTokenServiceTests
{
    private static readonly JwtSettings Settings = new()
    {
        Secret = "chave-de-teste-com-mais-de-trinta-e-dois-caracteres",
        Issuer = "Oficina.Api",
        Audience = "Oficina.Api",
        ExpirationMinutes = 60
    };

    private static JwtTokenService CriarService() => new(Options.Create(Settings));

    [Fact]
    public void GerarToken_DeveConterClaimsDoUsuario_EExpirarEmUmaHora()
    {
        var usuario = new Usuario("Maria", "maria", "hash", EnumsUsuario.PerfilUsuario.Administrador);

        var antes = DateTime.UtcNow;
        var resultado = CriarService().GerarToken(usuario);

        var token = new JsonWebTokenHandler().ReadJsonWebToken(resultado.Token);

        Assert.Equal("maria", token.GetClaim(JwtTokenService.NameClaim).Value);
        Assert.Equal("Administrador", token.GetClaim(JwtTokenService.RoleClaim).Value);
        Assert.Equal("Oficina.Api", token.Issuer);
        Assert.InRange(resultado.ExpiraEm, antes.AddMinutes(59), antes.AddMinutes(61));
    }

    [Fact]
    public async Task GerarToken_DeveGerarTokenValidoParaAChaveConfigurada()
    {
        var usuario = new Usuario("Joao", "joao", "hash", EnumsUsuario.PerfilUsuario.Mecanico);

        var resultado = CriarService().GerarToken(usuario);

        var validacao = await new JsonWebTokenHandler().ValidateTokenAsync(resultado.Token, new TokenValidationParameters
        {
            ValidIssuer = Settings.Issuer,
            ValidAudience = Settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.Secret)),
            RoleClaimType = JwtTokenService.RoleClaim
        });

        Assert.True(validacao.IsValid);
        Assert.True(validacao.ClaimsIdentity.IsAuthenticated);
        Assert.True(new System.Security.Claims.ClaimsPrincipal(validacao.ClaimsIdentity).IsInRole("Mecanico"));
    }
}
