using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Oficina.Application.DTOs.Usuario;
using Oficina.Application.Mappings;
using Oficina.Domain.Enums;

namespace Oficina.Application.UnitTests.Usuario;

public class UsuarioProfileTests
{
    private static MapperConfiguration CriarConfiguracao()
    {
        var expression = new MapperConfigurationExpression();
        expression.AddProfile<UsuarioProfile>();

        return new MapperConfiguration(expression, NullLoggerFactory.Instance);
    }

    [Fact]
    public void Configuracao_DeveSerValida()
    {
        CriarConfiguracao().AssertConfigurationIsValid();
    }

    [Fact]
    public void Usuario_DeveMapearNomeUserNameEPerfil()
    {
        var usuario = new Domain.Entities.Usuario("Maria", "maria", "hash", EnumsUsuario.PerfilUsuario.Mecanico);

        var dto = CriarConfiguracao().CreateMapper().Map<UsuarioDto>(usuario);

        Assert.Equal("Maria", dto.Nome);
        Assert.Equal("maria", dto.UserName);
        Assert.Equal(EnumsUsuario.PerfilUsuario.Mecanico, dto.Perfil);
    }

    [Fact]
    public void Usuario_DeveMapearColecao()
    {
        var usuarios = new[]
        {
            new Domain.Entities.Usuario("Maria", "maria", "hash", EnumsUsuario.PerfilUsuario.Mecanico),
            new Domain.Entities.Usuario("Admin", "admin", "hash", EnumsUsuario.PerfilUsuario.Administrador)
        };

        var dtos = CriarConfiguracao().CreateMapper().Map<IEnumerable<UsuarioDto>>(usuarios).ToList();

        Assert.Equal(new[] { "maria", "admin" }, dtos.Select(d => d.UserName));
    }
}
