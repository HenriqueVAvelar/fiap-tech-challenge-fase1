using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Oficina.Application.Mappings;
using Oficina.Domain.Entities;
using CatalogoEntity = Oficina.Domain.Entities.Catalogo;
using Oficina.Domain.Interfaces;

namespace Oficina.Application.UnitTests.Fakes;

internal static class CadastroFakes
{
    public static IMapper CriarMapper()
    {
        var expression = new MapperConfigurationExpression();
        expression.AddProfile<CatalogoProfile>();
        expression.AddProfile<ClienteProfile>();
        expression.AddProfile<VeiculoProfile>();
        expression.AddProfile<ServicoProfile>();
        expression.AddProfile<EstoqueProfile>();

        return new MapperConfiguration(expression, NullLoggerFactory.Instance).CreateMapper();
    }

    public static void DefinirId<T>(T entidade, int id) where T : class
        => typeof(T).GetProperty("Id")!.SetValue(entidade, id);
}

internal sealed class FakeCatalogoRepository : ICatalogoRepository
{
    private int _proximoId = 1;
    public List<CatalogoEntity> Itens { get; } = new();
    public int Atualizacoes { get; private set; }

    public Task<IEnumerable<CatalogoEntity>> ObterTodosAsync() => Task.FromResult<IEnumerable<CatalogoEntity>>(Itens.ToList());

    public Task<CatalogoEntity?> ObterPorIdAsync(int id) => Task.FromResult(Itens.FirstOrDefault(c => c.Id == id));

    public Task AdicionarAsync(CatalogoEntity catalogo)
    {
        CadastroFakes.DefinirId(catalogo, _proximoId++);
        Itens.Add(catalogo);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(CatalogoEntity catalogo)
    {
        Atualizacoes++;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(int id)
    {
        Itens.RemoveAll(c => c.Id == id);
        return Task.CompletedTask;
    }
}

internal sealed class FakeClienteRepository : IClienteRepository
{
    private int _proximoId = 1;
    public List<Cliente> Clientes { get; } = new();

    public Task<IEnumerable<Cliente>> ObterTodosAsync() => Task.FromResult<IEnumerable<Cliente>>(Clientes.ToList());

    public Task<Cliente?> ObterPorIdAsync(int id) => Task.FromResult(Clientes.FirstOrDefault(c => c.Id == id));

    public Task<Cliente?> ObterPorDocumentoAsync(string numero)
        => Task.FromResult(Clientes.FirstOrDefault(c => c.Documento.Numero == numero));

    public Task<bool> DocumentoExisteAsync(string numero, int? ignorarId = null)
        => Task.FromResult(Clientes.Any(c => c.Documento.Numero == numero && (ignorarId == null || c.Id != ignorarId)));

    public Task AdicionarAsync(Cliente cliente)
    {
        cliente.Id = _proximoId++;
        Clientes.Add(cliente);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Cliente cliente) => Task.CompletedTask;

    public Task RemoverAsync(int id)
    {
        Clientes.RemoveAll(c => c.Id == id);
        return Task.CompletedTask;
    }
}

internal sealed class FakeVeiculoRepository : IVeiculoRepository
{
    private int _proximoId = 1;
    public List<Veiculo> Veiculos { get; } = new();

    public Task<IEnumerable<Veiculo>> ObterTodosAsync() => Task.FromResult<IEnumerable<Veiculo>>(Veiculos.ToList());

    public Task<Veiculo?> ObterPorIdAsync(int id) => Task.FromResult(Veiculos.FirstOrDefault(v => v.Id == id));

    public Task<Veiculo?> ObterPorPlacaAsync(string placa)
        => Task.FromResult(Veiculos.FirstOrDefault(v => v.Placa.Valor == placa));

    public Task<bool> PlacaExisteAsync(string placa, int? ignorarId = null)
        => Task.FromResult(Veiculos.Any(v => v.Placa.Valor == placa && (ignorarId == null || v.Id != ignorarId)));

    public Task AdicionarAsync(Veiculo veiculo)
    {
        veiculo.Id = _proximoId++;
        Veiculos.Add(veiculo);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Veiculo veiculo) => Task.CompletedTask;

    public Task RemoverAsync(int id)
    {
        Veiculos.RemoveAll(v => v.Id == id);
        return Task.CompletedTask;
    }
}

internal sealed class FakeServicoRepository : IServicoRepository
{
    private int _proximoId = 1;
    public List<Servico> Servicos { get; } = new();

    public Task<IEnumerable<Servico>> ObterTodosAsync() => Task.FromResult<IEnumerable<Servico>>(Servicos.ToList());

    public Task<Servico?> ObterPorIdAsync(int id) => Task.FromResult(Servicos.FirstOrDefault(s => s.Id == id));

    public Task AdicionarAsync(Servico servico)
    {
        servico.Id = _proximoId++;
        Servicos.Add(servico);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Servico servico) => Task.CompletedTask;

    public Task RemoverAsync(int id)
    {
        Servicos.RemoveAll(s => s.Id == id);
        return Task.CompletedTask;
    }
}

internal sealed class FakeEstoqueRepository : IEstoqueRepository
{
    private int _proximoId = 1;
    public List<Estoque> Estoques { get; } = new();

    public Task<IEnumerable<Estoque>> ObterTodosAsync() => Task.FromResult<IEnumerable<Estoque>>(Estoques.ToList());

    public Task<Estoque?> ObterPorIdAsync(int id) => Task.FromResult(Estoques.FirstOrDefault(e => e.Id == id));

    public Task<Estoque?> ObterPorCatalogoIdAsync(int catalogoId)
        => Task.FromResult(Estoques.FirstOrDefault(e => e.CatalogoId == catalogoId));

    public Task AdicionarAsync(Estoque estoque)
    {
        estoque.Id = _proximoId++;
        Estoques.Add(estoque);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Estoque estoque) => Task.CompletedTask;

    public Task RemoverAsync(int id)
    {
        Estoques.RemoveAll(e => e.Id == id);
        return Task.CompletedTask;
    }
}
