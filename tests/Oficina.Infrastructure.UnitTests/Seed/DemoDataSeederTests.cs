using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oficina.Application.Extensions;
using Oficina.Domain.Entities;
using Oficina.Domain.Enums;
using Oficina.Domain.Interfaces;
using Oficina.Infrastructure.Seed;
using Oficina.Infrastructure.UnitTests.Support;

namespace Oficina.Infrastructure.UnitTests.Seed;

public class DemoDataSeederTests
{
    private const int TotalItensPadrao = 10;

    private readonly RepositoriosEmMemoria _repositorios = new();
    private readonly LoggerFactoryCapture _logs = new();

    private IServiceProvider CriarServicos(bool habilitado)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(_logs);
        services.AddSingleton<IOptions<DemoDataSettings>>(Options.Create(new DemoDataSettings { Habilitado = habilitado }));
        services.AddApplication();
        services.AddSingleton<ICatalogoRepository>(_repositorios.Catalogos);
        services.AddSingleton<IEstoqueRepository>(_repositorios.Estoques);
        services.AddSingleton<IClienteRepository>(_repositorios.Clientes);
        services.AddSingleton<IVeiculoRepository>(_repositorios.Veiculos);
        services.AddSingleton<IServicoRepository>(_repositorios.Servicos);
        services.AddSingleton<IOrdemServicoRepository>(new RepositoriosEmMemoria.OrdensVazias());
        services.AddSingleton<IUsuarioRepository>(new RepositoriosEmMemoria.UsuariosVazios());
        services.AddSingleton<Oficina.Application.Interfaces.IPasswordHasher>(new RepositoriosEmMemoria.HasherFalso());
        services.AddSingleton<Oficina.Application.Interfaces.ITokenService>(new RepositoriosEmMemoria.TokenFalso());

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Seed_DeveCriarAsPecasEInsumosPadrao_ComEstoqueInicial_MesmoComDemoDesabilitado()
    {
        await CriarServicos(habilitado: false).SeedDadosDemoAsync();

        Assert.Equal(TotalItensPadrao, _repositorios.Catalogos.Itens.Count);
        Assert.Equal(5, _repositorios.Catalogos.Itens.Count(c => c.Tipo == EnumsConsumivel.TipoConsumivel.Peca));
        Assert.Equal(5, _repositorios.Catalogos.Itens.Count(c => c.Tipo == EnumsConsumivel.TipoConsumivel.Insumo));
        Assert.All(_repositorios.Catalogos.Itens, c => Assert.True(c.Ativo));

        Assert.Equal(TotalItensPadrao, _repositorios.Estoques.Itens.Count);
        Assert.All(_repositorios.Estoques.Itens, e => Assert.Equal(10, e.Quantidade));
        Assert.Equal(
            _repositorios.Catalogos.Itens.Select(c => c.Id).OrderBy(id => id),
            _repositorios.Estoques.Itens.Select(e => e.CatalogoId).OrderBy(id => id));
    }

    [Fact]
    public async Task Seed_NaoDeveCriarClienteVeiculoNemServico_QuandoDemoDesabilitado()
    {
        await CriarServicos(habilitado: false).SeedDadosDemoAsync();

        Assert.Empty(_repositorios.Clientes.Itens);
        Assert.Empty(_repositorios.Veiculos.Itens);
        Assert.Empty(_repositorios.Servicos.Itens);
        Assert.DoesNotContain(_repositorios.Catalogos.Itens, c => c.Nome == "Motor");
    }

    [Fact]
    public async Task Seed_DeveCriarOsDadosDeDemonstracao_QuandoHabilitado()
    {
        await CriarServicos(habilitado: true).SeedDadosDemoAsync();

        var cliente = Assert.Single(_repositorios.Clientes.Itens);
        Assert.Equal("40536649855", cliente.Documento.Numero);

        var veiculo = Assert.Single(_repositorios.Veiculos.Itens);
        Assert.Equal(cliente.Id, veiculo.ClienteId);
        Assert.Equal("DTY0911", veiculo.Placa.Valor);

        var motor = Assert.Single(_repositorios.Catalogos.Itens, c => c.Nome == "Motor");
        Assert.Equal(5000m, motor.Preco);
        Assert.Equal(TotalItensPadrao + 1, _repositorios.Catalogos.Itens.Count);
        Assert.Equal(10, Assert.Single(_repositorios.Estoques.Itens, e => e.CatalogoId == motor.Id).Quantidade);

        var servico = Assert.Single(_repositorios.Servicos.Itens);
        Assert.Equal("Revisão completa", servico.Descricao);
        Assert.Equal(5000m, servico.Valor);
        Assert.True(servico.Ativo);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Seed_DeveSerIdempotente_QuandoExecutadoMaisDeUmaVez(bool habilitado)
    {
        var servicos = CriarServicos(habilitado);

        await servicos.SeedDadosDemoAsync();
        var catalogos = _repositorios.Catalogos.Itens.Count;
        var estoques = _repositorios.Estoques.Itens.Count;
        var clientes = _repositorios.Clientes.Itens.Count;
        var veiculos = _repositorios.Veiculos.Itens.Count;
        var servicosCriados = _repositorios.Servicos.Itens.Count;

        await servicos.SeedDadosDemoAsync();

        Assert.Equal(catalogos, _repositorios.Catalogos.Itens.Count);
        Assert.Equal(estoques, _repositorios.Estoques.Itens.Count);
        Assert.Equal(clientes, _repositorios.Clientes.Itens.Count);
        Assert.Equal(veiculos, _repositorios.Veiculos.Itens.Count);
        Assert.Equal(servicosCriados, _repositorios.Servicos.Itens.Count);
    }

    [Fact]
    public async Task Seed_DeveCriarSomenteOEstoqueQueFalta_QuandoOItemJaExisteNoCatalogo()
    {
        var existente = new Catalogo("Alternador", 900m, EnumsConsumivel.TipoConsumivel.Peca, true);
        await _repositorios.Catalogos.AdicionarAsync(existente);

        await CriarServicos(habilitado: false).SeedDadosDemoAsync();

        Assert.Equal(TotalItensPadrao, _repositorios.Catalogos.Itens.Count);
        Assert.Single(_repositorios.Catalogos.Itens, c => c.Nome == "Alternador");
        Assert.Single(_repositorios.Estoques.Itens, e => e.CatalogoId == existente.Id);
    }

    [Fact]
    public async Task Seed_NaoDeveRecriarOEstoque_QuandoOItemJaTemEstoque()
    {
        var existente = new Catalogo("Alternador", 900m, EnumsConsumivel.TipoConsumivel.Peca, true);
        await _repositorios.Catalogos.AdicionarAsync(existente);
        await _repositorios.Estoques.AdicionarAsync(new Estoque(3) { CatalogoId = existente.Id });

        await CriarServicos(habilitado: false).SeedDadosDemoAsync();

        var estoque = Assert.Single(_repositorios.Estoques.Itens, e => e.CatalogoId == existente.Id);
        Assert.Equal(3, estoque.Quantidade);
    }

    [Fact]
    public async Task Seed_NaoDeveLancarExcecao_QuandoUmGrupoFalha_EContinuaComOsDemais()
    {
        _repositorios.Clientes.FalharAoVerificarDocumento = true;

        await CriarServicos(habilitado: true).SeedDadosDemoAsync();

        Assert.Contains(_logs.Entradas, e => e.Nivel == LogLevel.Error);
        Assert.Empty(_repositorios.Clientes.Itens);
        Assert.Single(_repositorios.Servicos.Itens);
        Assert.Contains(_repositorios.Catalogos.Itens, c => c.Nome == "Motor");
    }

    [Fact]
    public async Task Seed_NaoDeveLancarExcecao_QuandoOCatalogoPadraoFalha()
    {
        _repositorios.Catalogos.FalharAoObterTodos = true;

        await CriarServicos(habilitado: false).SeedDadosDemoAsync();

        Assert.Contains(_logs.Entradas, e => e.Nivel == LogLevel.Error && e.Excecao is InvalidOperationException);
        Assert.Empty(_repositorios.Estoques.Itens);
    }
}
