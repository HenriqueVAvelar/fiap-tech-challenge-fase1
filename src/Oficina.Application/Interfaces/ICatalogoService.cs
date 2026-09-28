using Oficina.Application.DTOs.Catalogo;

namespace Oficina.Application.Interfaces;

public interface ICatalogoService
{
    Task<IEnumerable<CatalogoDto>> ObterTodosAsync();

    Task<CatalogoDto?> ObterPorIdAsync(int id);

    Task<CatalogoDto> CriarAsync(CatalogoInputDto catalogo);

    Task<bool> AtualizarAsync(int id, CatalogoInputDto catalogo);

    Task RemoverAsync(int id);
}
