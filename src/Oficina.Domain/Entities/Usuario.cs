using Oficina.Domain.Enums;

namespace Oficina.Domain.Entities;

public class Usuario
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public string UserName { get; private set; }
    public string SenhaHash { get; private set; }
    public EnumsUsuario.PerfilUsuario Perfil { get; private set; }

    private Usuario()
    {
        Nome = string.Empty;
        UserName = string.Empty;
        SenhaHash = string.Empty;
    }

    public Usuario(string nome, string userName, string senhaHash, EnumsUsuario.PerfilUsuario perfil)
    {
        ValidarInvariantes(nome, userName, perfil);
        ValidarSenhaHash(senhaHash);

        Nome = nome.Trim();
        UserName = userName.Trim();
        SenhaHash = senhaHash;
        Perfil = perfil;
    }

    public void Atualizar(string nome, string userName, EnumsUsuario.PerfilUsuario perfil)
    {
        ValidarInvariantes(nome, userName, perfil);

        Nome = nome.Trim();
        UserName = userName.Trim();
        Perfil = perfil;
    }

    public void AlterarSenha(string senhaHash)
    {
        ValidarSenhaHash(senhaHash);

        SenhaHash = senhaHash;
    }

    public static void ValidarSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
            throw new ArgumentException("A senha é obrigatória.", nameof(senha));

        if (!senha.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
            throw new ArgumentException("A senha deve conter ao menos um caractere especial.", nameof(senha));
    }

    private static void ValidarInvariantes(string nome, string userName, EnumsUsuario.PerfilUsuario perfil)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do usuário é obrigatório.", nameof(nome));

        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("O UserName do usuário é obrigatório.", nameof(userName));

        if (!Enum.IsDefined(perfil))
            throw new ArgumentException("O perfil do usuário é inválido.", nameof(perfil));
    }

    private static void ValidarSenhaHash(string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ArgumentException("A senha é obrigatória.", nameof(senhaHash));
    }
}
