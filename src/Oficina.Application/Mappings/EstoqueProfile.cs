using AutoMapper;
using Oficina.Application.DTOs;
using Oficina.Domain.Entities;

namespace Oficina.Application.Mappings;

public class EstoqueProfile : Profile
{
    public EstoqueProfile()
    {
        CreateMap<Estoque, EstoqueDto>();
    }
}
