using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Oficina.Api.Controllers;

/// <summary>
/// Base dos controllers: o método ExecutarAsync roda a ação e transforma as exceções
/// de regra de negócio em respostas HTTP, evitando um try/catch em cada endpoint.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    protected async Task<IActionResult> ExecutarAsync(Func<Task<IActionResult>> acao)
    {
        try
        {
            return await acao();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (DbUpdateException)
        {
            return Conflict("A operação conflita com dados existentes (registro duplicado ou em uso por outro cadastro).");
        }
    }
}
