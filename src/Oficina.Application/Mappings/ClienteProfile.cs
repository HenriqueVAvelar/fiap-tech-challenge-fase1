using AutoMapper;
using Oficina.Application.DTOs;
using Oficina.Domain.Entities;

namespace Oficina.Application.Mappings;

public class ClienteProfile : Profile
{
    public ClienteProfile()
    {
        CreateMap<Cliente, ClienteDto>()
            .ForMember(d => d.Documento, o => o.MapFrom(s => s.Documento.Numero))
            .ForMember(d => d.TipoDocumento, o => o.MapFrom(s => s.Documento.Tipo));
    }
}
