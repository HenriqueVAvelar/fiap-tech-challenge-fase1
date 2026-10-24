using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oficina.Application.DTOs;
using Oficina.Application.DTOs.Catalogo;
using Oficina.Application.Interfaces;
using Oficina.Domain.Enums;
using Oficina.Domain.Interfaces;

namespace Oficina.Infrastructure.Seed;

public class DemoDataSettings
{
    public const string SectionName = "DemoData";

    public bool Habilitado { get; set; }
}

public static class DemoDataSeeder
{
    private const string DocumentoDemo = "40536649855";
    private const string CatalogoDemo = "Motor";
    private const string ServicoDemo = "Revisão completa";
    private const int QuantidadeInicialEstoque = 10;

    private static readonly (string Nome, decimal Preco, EnumsConsumivel.TipoConsumivel Tipo)[] ItensPadrao =
    [
        ("Bateria automotiva", 500m, EnumsConsumivel.TipoConsumivel.Peca),
        ("Alternador", 900m, EnumsConsumivel.TipoConsumivel.Peca),
        ("Amortecedor dianteiro", 300m, EnumsConsumivel.TipoConsumivel.Peca),
        ("Kit de embreagem", 700m, EnumsConsumivel.TipoConsumivel.Peca),
        ("Farol dianteiro", 400m, EnumsConsumivel.TipoConsumivel.Peca),

        ("Óleo de motor (1 litro)", 40m, EnumsConsumivel.TipoConsumivel.Insumo),
        ("Líquido de arrefecimento (1 litro)", 25m, EnumsConsumivel.TipoConsumivel.Insumo),
        ("Fluido de freio (500 ml)", 25m, EnumsConsumivel.TipoConsumivel.Insumo),
        ("Graxa lubrificante (500 g)", 35m, EnumsConsumivel.TipoConsumivel.Insumo),
        ("Água desmineralizada (1 litro)", 8m, EnumsConsumivel.TipoConsumivel.Insumo)
    ];

    /// <summary>
    /// Cria os dados padrão: sempre as peças e insumos do catálogo (com estoque inicial) e, quando a seção
    /// "DemoData:Habilitado" da configuração estiver ligada, também dados de demonstração:
    /// um cliente e um veículo; um item de catálogo com seu estoque; e um serviço.
    /// Cada grupo só é criado se ainda não existir, então é seguro rodar a cada inicialização.
    /// </summary>
    public static async Task SeedDadosDemoAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var settings = scope.ServiceProvider.GetRequiredService<IOptions<DemoDataSettings>>().Value;
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DemoDataSeeder));

        await ExecutarAsync(logger, "peças e insumos padrão", () => CriarCatalogoPadraoAsync(scope.ServiceProvider, logger));

        if (!settings.Habilitado)
            return;

        await ExecutarAsync(logger, "cliente e veículo", () => CriarClienteEVeiculoAsync(scope.ServiceProvider, logger));
        await ExecutarAsync(logger, "catálogo e estoque", () => CriarCatalogoEEstoqueAsync(scope.ServiceProvider, logger));
        await ExecutarAsync(logger, "serviço", () => CriarServicoAsync(scope.ServiceProvider, logger));
    }

    private static async Task ExecutarAsync(ILogger logger, string grupo, Func<Task> criar)
    {
        try
        {
            await criar();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Não foi possível criar os dados de demonstração ({Grupo}).", grupo);
        }
    }

    private static async Task CriarCatalogoPadraoAsync(IServiceProvider sp, ILogger logger)
    {
        var catalogoService = sp.GetRequiredService<ICatalogoService>();
        var estoqueService = sp.GetRequiredService<IEstoqueService>();
        var estoqueRepository = sp.GetRequiredService<IEstoqueRepository>();

        var existentes = (await catalogoService.ObterTodosAsync()).ToList();

        foreach (var (nome, preco, tipo) in ItensPadrao)
        {
            var catalogoId = existentes.FirstOrDefault(c => c.Nome == nome)?.Id
                ?? (await catalogoService.CriarAsync(new CatalogoInputDto
                {
                    Nome = nome,
                    Preco = preco,
                    Tipo = tipo,
                    Ativo = true,
                    Descricao = ""
                })).Id;

            if (await estoqueRepository.ObterPorCatalogoIdAsync(catalogoId) is not null)
                continue;

            await estoqueService.CriarAsync(new EstoqueInputDto
            {
                CatalogoId = catalogoId,
                Quantidade = QuantidadeInicialEstoque
            });

            logger.LogInformation("Catálogo padrão: '{Nome}' ({CatalogoId}) criado com estoque.", nome, catalogoId);
        }
    }

    private static async Task CriarClienteEVeiculoAsync(IServiceProvider sp, ILogger logger)
    {
        var clienteRepository = sp.GetRequiredService<IClienteRepository>();

        if (await clienteRepository.DocumentoExisteAsync(DocumentoDemo))
            return;

        var clienteService = sp.GetRequiredService<IClienteService>();
        var veiculoService = sp.GetRequiredService<IVeiculoService>();

        // Os ids são gerados pelo banco ao gravar; cada passo usa o id devolvido pelo anterior.
        var cliente = await clienteService.CriarAsync(new ClienteInputDto
        {
            Nome = "Cristano Ronaldo",
            Documento = DocumentoDemo
        });

        var veiculo = await veiculoService.CriarAsync(new VeiculoInputDto
        {
            ClienteId = cliente.Id,
            Placa = "DTY0911",
            Marca = "Ferrari",
            Modelo = "Roma",
            Ano = 2025
        });

        logger.LogInformation(
            "Demonstração criada: cliente {ClienteId}, veículo {VeiculoId}.",
            cliente.Id, veiculo.Id);
    }

    private static async Task CriarCatalogoEEstoqueAsync(IServiceProvider sp, ILogger logger)
    {
        var catalogoService = sp.GetRequiredService<ICatalogoService>();
        var estoqueService = sp.GetRequiredService<IEstoqueService>();
        var estoqueRepository = sp.GetRequiredService<IEstoqueRepository>();

        var existente = (await catalogoService.ObterTodosAsync())
            .FirstOrDefault(c => c.Nome == CatalogoDemo);

        var catalogoId = existente?.Id ?? (await catalogoService.CriarAsync(new CatalogoInputDto
        {
            Nome = CatalogoDemo,
            Preco = 5000.00m,
            Tipo = EnumsConsumivel.TipoConsumivel.Peca,
            Ativo = true,
            Descricao = ""
        })).Id;

        if (await estoqueRepository.ObterPorCatalogoIdAsync(catalogoId) is not null)
            return;

        var estoque = await estoqueService.CriarAsync(new EstoqueInputDto
        {
            CatalogoId = catalogoId,
            Quantidade = 10
        });

        logger.LogInformation(
            "Demonstração criada: catálogo {CatalogoId} ('{Nome}') com estoque {EstoqueId}.",
            catalogoId, CatalogoDemo, estoque.Id);
    }

    private static async Task CriarServicoAsync(IServiceProvider sp, ILogger logger)
    {
        var servicoService = sp.GetRequiredService<IServicoService>();

        if ((await servicoService.ObterTodosAsync()).Any(s => s.Descricao == ServicoDemo))
            return;

        var servico = await servicoService.CriarAsync(new ServicoInputDto
        {
            Descricao = ServicoDemo,
            Valor = 5000.00m,
            Ativo = true
        });

        logger.LogInformation("Demonstração criada: serviço {ServicoId} ('{Descricao}').", servico.Id, ServicoDemo);
    }
}
