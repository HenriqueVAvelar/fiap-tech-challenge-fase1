namespace Oficina.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string senha);

    bool Verificar(string senhaHash, string senhaInformada);
}
