using Oficina.Application.DTOs;

namespace Oficina.Application.Interfaces;

public interface IEstoqueService
{
    Task<IEnumerable<EstoqueDto>> ObterTodosAsync();

    Task<EstoqueDto?> ObterPorIdAsync(int id);

    Task<EstoqueDto> CriarAsync(EstoqueInputDto estoque);

    Task<bool> AtualizarAsync(int id, EstoqueUpdateInputDto estoque);

    Task<bool> RemoverAsync(int id);

    Task<EstoqueDto?> RegistrarEntradaAsync(int id, int quantidade);

    Task<EstoqueDto?> RegistrarBaixaAsync(int id, int quantidade);
}
