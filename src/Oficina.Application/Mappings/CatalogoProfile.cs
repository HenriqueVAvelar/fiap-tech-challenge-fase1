using AutoMapper;
using Oficina.Application.DTOs.Catalogo;
using Oficina.Domain.Entities;

namespace Oficina.Application.Mappings;

public class CatalogoProfile : Profile
{
    public CatalogoProfile()
    {
        CreateMap<Catalogo, CatalogoDto>();

        CreateMap<CatalogoInputDto, Catalogo>()
            .ConstructUsing(dto => new Catalogo(
                dto.Nome,
                dto.Preco,
                dto.Tipo,
                dto.Ativo,
                dto.Descricao))
            .ForAllMembers(opt => opt.Ignore());
    }
}
