using AutoMapper;
using Oficina.Application.DTOs;
using Oficina.Domain.Entities;

namespace Oficina.Application.Mappings;

public class VeiculoProfile : Profile
{
    public VeiculoProfile()
    {
        CreateMap<Veiculo, VeiculoDto>()
            .ForMember(d => d.Placa, o => o.MapFrom(s => s.Placa.Valor));
    }
}
