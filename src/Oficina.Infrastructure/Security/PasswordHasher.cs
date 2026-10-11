using Microsoft.AspNetCore.Identity;
using Oficina.Application.Interfaces;

namespace Oficina.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string senha) => _hasher.HashPassword(null!, senha);

    public bool Verificar(string senhaHash, string senhaInformada)
        => _hasher.VerifyHashedPassword(null!, senhaHash, senhaInformada)
            is not PasswordVerificationResult.Failed;
}
