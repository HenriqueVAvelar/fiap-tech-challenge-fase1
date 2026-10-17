using AutoMapper;
using Oficina.Application.DTOs;
using Oficina.Domain.Entities;

namespace Oficina.Application.Mappings;

public class ServicoProfile : Profile
{
    public ServicoProfile()
    {
        CreateMap<Servico, ServicoDto>();
    }
}
