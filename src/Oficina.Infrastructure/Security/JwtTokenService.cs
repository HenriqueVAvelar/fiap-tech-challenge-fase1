using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Oficina.Application.DTOs.Auth;
using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;

namespace Oficina.Infrastructure.Security;

public class JwtTokenService : ITokenService
{
    // Nomes das claims. A API valida o token usando os mesmos nomes (JwtAuthenticationExtensions).
    public const string RoleClaim = "role";
    public const string NameClaim = "name";

    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public LoginResultDto GerarToken(Usuario usuario)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(NameClaim, usuario.UserName),
                new Claim(RoleClaim, usuario.Perfil.ToString())
            }),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = expiraEm,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new LoginResultDto { Token = token, ExpiraEm = expiraEm };
    }
}
