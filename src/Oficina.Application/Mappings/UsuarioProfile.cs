using AutoMapper;
using Oficina.Application.DTOs.Usuario;
using Oficina.Domain.Entities;

namespace Oficina.Application.Mappings;

public class UsuarioProfile : Profile
{
    public UsuarioProfile()
    {
        CreateMap<Usuario, UsuarioDto>();
    }
}
