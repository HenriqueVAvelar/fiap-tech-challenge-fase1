using System.Net;
using Oficina.Api.IntegrationTests.Support;
using Oficina.Application.DTOs.Usuario;
using Oficina.Domain.Enums;

namespace Oficina.Api.IntegrationTests;

[Collection(ApiCollection.Nome)]
public class UsuariosIntegrationTests : IntegrationTestBase
{
    private const string SenhaValida = "Senha@123";

    public UsuariosIntegrationTests(OficinaApiFactory factory) : base(factory)
    {
    }

    private static UsuarioInputDto NovoUsuario(string? userName = null, string senha = SenhaValida)
        => new()
        {
            Nome = "Usuário de Integração",
            UserName = userName ?? DadosDeTeste.Texto("usuario"),
            Senha = senha,
            Perfil = EnumsUsuario.PerfilUsuario.Mecanico
        };

    private static async Task<UsuarioDto> CriarUsuarioAsync(HttpClient admin, UsuarioInputDto? input = null)
    {
        var resposta = await (await admin.PostJsonAsync("/Usuarios", input ?? NovoUsuario()))
            .DeveTerStatusAsync(HttpStatusCode.Created);

        return await resposta.LerAsync<UsuarioDto>();
    }

    [Fact]
    public async Task Listar_ComoAdministrador_RetornaOsUsuariosSemExporSenha()
    {
        var admin = await ClienteAdminAsync();
        var criado = await CriarUsuarioAsync(admin);

        var resposta = await (await admin.GetAsync("/Usuarios")).DeveTerStatusAsync(HttpStatusCode.OK);

        resposta.DeveSerJson();
        var usuarios = await resposta.LerAsync<List<UsuarioDto>>();
        Assert.Contains(usuarios, u => u.Id == criado.Id && u.UserName == criado.UserName);
        Assert.DoesNotContain("senha", (await resposta.Content.ReadAsStringAsync()).ToLowerInvariant());
    }

    [Fact]
    public async Task Listar_ComoMecanico_Retorna200()
    {
        var mecanico = await ClienteMecanicoAsync();

        var resposta = await mecanico.GetAsync("/Usuarios");

        await resposta.DeveTerStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Listar_SemToken_Retorna401()
    {
        var resposta = await ClienteAnonimo().GetAsync("/Usuarios");

        await resposta.DeveTerStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObterPorId_Existente_RetornaOUsuario()
    {
        var admin = await ClienteAdminAsync();
        var criado = await CriarUsuarioAsync(admin);

        var resposta = await (await admin.GetAsync($"/Usuarios/{criado.Id}")).DeveTerStatusAsync(HttpStatusCode.OK);

        var usuario = await resposta.LerAsync<UsuarioDto>();
        Assert.Equal(criado.Id, usuario.Id);
        Assert.Equal(criado.UserName, usuario.UserName);
        Assert.Equal(EnumsUsuario.PerfilUsuario.Mecanico, usuario.Perfil);
    }

    [Fact]
    public async Task ObterPorId_Inexistente_Retorna404()
    {
        var admin = await ClienteAdminAsync();

        var resposta = await admin.GetAsync("/Usuarios/999999");

        await resposta.DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Criar_ComoAdministrador_Retorna201ComLocation()
    {
        var admin = await ClienteAdminAsync();
        var input = NovoUsuario();

        var resposta = await (await admin.PostJsonAsync("/Usuarios", input)).DeveTerStatusAsync(HttpStatusCode.Created);

        var usuario = await resposta.LerAsync<UsuarioDto>();
        Assert.True(usuario.Id > 0);
        Assert.Equal(input.UserName, usuario.UserName);
        Assert.EndsWith($"/Usuarios/{usuario.Id}", resposta.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Criar_ComoMecanico_Retorna403()
    {
        var mecanico = await ClienteMecanicoAsync();

        var resposta = await mecanico.PostJsonAsync("/Usuarios", NovoUsuario());

        await resposta.DeveTerStatusAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Criar_ComUserNameJaEmUso_Retorna409()
    {
        var admin = await ClienteAdminAsync();
        var existente = await CriarUsuarioAsync(admin);

        var resposta = await admin.PostJsonAsync("/Usuarios", NovoUsuario(existente.UserName.ToUpperInvariant()));

        await resposta.DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Criar_ComSenhaSemCaractereEspecial_Retorna400()
    {
        var admin = await ClienteAdminAsync();

        var resposta = await admin.PostJsonAsync("/Usuarios", NovoUsuario(senha: "Senha123"));

        await resposta.DeveTerStatusAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Atualizar_ComoAdministrador_Retorna204_EAlteraOsDados()
    {
        var admin = await ClienteAdminAsync();
        var criado = await CriarUsuarioAsync(admin);
        var novoUserName = DadosDeTeste.Texto("renomeado");

        var resposta = await admin.PutJsonAsync($"/Usuarios/{criado.Id}", new UsuarioUpdateInputDto
        {
            Nome = "Nome Alterado",
            UserName = novoUserName,
            Perfil = EnumsUsuario.PerfilUsuario.Administrador
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.NoContent);

        var atualizado = await (await (await admin.GetAsync($"/Usuarios/{criado.Id}"))
            .DeveTerStatusAsync(HttpStatusCode.OK)).LerAsync<UsuarioDto>();
        Assert.Equal("Nome Alterado", atualizado.Nome);
        Assert.Equal(novoUserName, atualizado.UserName);
        Assert.Equal(EnumsUsuario.PerfilUsuario.Administrador, atualizado.Perfil);
    }

    [Fact]
    public async Task Atualizar_MantendoOProprioUserName_Retorna204()
    {
        var admin = await ClienteAdminAsync();
        var criado = await CriarUsuarioAsync(admin);

        var resposta = await admin.PutJsonAsync($"/Usuarios/{criado.Id}", new UsuarioUpdateInputDto
        {
            Nome = "Outro nome",
            UserName = criado.UserName,
            Perfil = EnumsUsuario.PerfilUsuario.Mecanico
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Atualizar_ComUserNameDeOutroUsuario_Retorna409()
    {
        var admin = await ClienteAdminAsync();
        var primeiro = await CriarUsuarioAsync(admin);
        var segundo = await CriarUsuarioAsync(admin);

        var resposta = await admin.PutJsonAsync($"/Usuarios/{segundo.Id}", new UsuarioUpdateInputDto
        {
            Nome = segundo.Nome,
            UserName = primeiro.UserName,
            Perfil = EnumsUsuario.PerfilUsuario.Mecanico
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Atualizar_ComSenhaInvalida_Retorna400()
    {
        var admin = await ClienteAdminAsync();
        var criado = await CriarUsuarioAsync(admin);

        var resposta = await admin.PutJsonAsync($"/Usuarios/{criado.Id}", new UsuarioUpdateInputDto
        {
            Nome = criado.Nome,
            UserName = criado.UserName,
            Senha = "SemEspecial1",
            Perfil = EnumsUsuario.PerfilUsuario.Mecanico
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Atualizar_ComNovaSenha_PermiteLoginComAChaveNova()
    {
        var admin = await ClienteAdminAsync();
        var criado = await CriarUsuarioAsync(admin);
        const string novaSenha = "Nova@Senha456";

        await (await admin.PutJsonAsync($"/Usuarios/{criado.Id}", new UsuarioUpdateInputDto
        {
            Nome = criado.Nome,
            UserName = criado.UserName,
            Senha = novaSenha,
            Perfil = EnumsUsuario.PerfilUsuario.Mecanico
        })).DeveTerStatusAsync(HttpStatusCode.NoContent);

        var cliente = await ClienteAutenticadoAsync(criado.UserName, novaSenha);

        await (await cliente.GetAsync("/Usuarios")).DeveTerStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Atualizar_Inexistente_Retorna404()
    {
        var admin = await ClienteAdminAsync();

        var resposta = await admin.PutJsonAsync("/Usuarios/999999", new UsuarioUpdateInputDto
        {
            Nome = "Ninguém",
            UserName = DadosDeTeste.Texto("nenhum"),
            Perfil = EnumsUsuario.PerfilUsuario.Mecanico
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Atualizar_ComoMecanico_Retorna403()
    {
        var admin = await ClienteAdminAsync();
        var criado = await CriarUsuarioAsync(admin);
        var mecanico = await ClienteMecanicoAsync();

        var resposta = await mecanico.PutJsonAsync($"/Usuarios/{criado.Id}", new UsuarioUpdateInputDto
        {
            Nome = criado.Nome,
            UserName = criado.UserName,
            Perfil = EnumsUsuario.PerfilUsuario.Mecanico
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Remover_ComoAdministrador_Retorna204_EDepoisRetorna404()
    {
        var admin = await ClienteAdminAsync();
        var criado = await CriarUsuarioAsync(admin);

        await (await admin.DeleteAsync($"/Usuarios/{criado.Id}")).DeveTerStatusAsync(HttpStatusCode.NoContent);

        await (await admin.GetAsync($"/Usuarios/{criado.Id}")).DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Remover_Inexistente_Retorna404()
    {
        var admin = await ClienteAdminAsync();

        var resposta = await admin.DeleteAsync("/Usuarios/999999");

        await resposta.DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Remover_ComoMecanico_Retorna403()
    {
        var admin = await ClienteAdminAsync();
        var criado = await CriarUsuarioAsync(admin);
        var mecanico = await ClienteMecanicoAsync();

        var resposta = await mecanico.DeleteAsync($"/Usuarios/{criado.Id}");

        await resposta.DeveTerStatusAsync(HttpStatusCode.Forbidden);
    }
}
