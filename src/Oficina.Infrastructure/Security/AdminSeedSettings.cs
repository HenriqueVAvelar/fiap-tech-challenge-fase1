namespace Oficina.Infrastructure.Security;

public class AdminSeedSettings
{
    public const string SectionName = "AdminSeed";

    public string Nome { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
