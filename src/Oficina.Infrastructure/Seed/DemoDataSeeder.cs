using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oficina.Application.DTOs;
using Oficina.Application.DTOs.Catalogo;
using Oficina.Application.DTOs.OrdemServico;
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

    /// <summary>
    /// Cria dados de demonstração quando a seção "DemoData:Habilitado" da configuração estiver ligada:
    /// um cliente, um veículo e uma Ordem de Serviço; um item de catálogo com seu estoque; e um serviço.
    /// Cada grupo só é criado se ainda não existir, então é seguro rodar a cada inicialização.
    /// </summary>
    public static async Task SeedDadosDemoAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var settings = scope.ServiceProvider.GetRequiredService<IOptions<DemoDataSettings>>().Value;
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DemoDataSeeder));

        if (!settings.Habilitado)
            return;

        await ExecutarAsync(logger, "cliente, veículo e OS", () => CriarClienteVeiculoOrdemAsync(scope.ServiceProvider, logger));
        await ExecutarAsync(logger, "catálogo e estoque", () => CriarCatalogoEEstoqueAsync(scope.ServiceProvider, logger));
        await ExecutarAsync(logger, "serviço", () => CriarServicoAsync(scope.ServiceProvider, logger));
        await ExecutarAsync(logger, "itens da OS", () => AdicionarItensNaOrdemAsync(scope.ServiceProvider, logger));
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

    private static async Task CriarClienteVeiculoOrdemAsync(IServiceProvider sp, ILogger logger)
    {
        var clienteRepository = sp.GetRequiredService<IClienteRepository>();

        if (await clienteRepository.DocumentoExisteAsync(DocumentoDemo))
            return;

        var clienteService = sp.GetRequiredService<IClienteService>();
        var veiculoService = sp.GetRequiredService<IVeiculoService>();
        var ordemService = sp.GetRequiredService<IOrdemServicoService>();

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

        var ordem = await ordemService.CriarAsync(new CriarOrdemServicoInputDto
        {
            Documento = cliente.Documento,
            Placa = veiculo.Placa
        });

        logger.LogInformation(
            "Demonstração criada: cliente {ClienteId}, veículo {VeiculoId}, OS {OrdemId}.",
            cliente.Id, veiculo.Id, ordem.Id);
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

    // Inclui na OS de demonstração o serviço e a peça (2 unidades do catálogo "Motor").
    // Só mexe na OS enquanto o orçamento ainda não foi gerado, e cada item só é incluído uma vez.
    private static async Task AdicionarItensNaOrdemAsync(IServiceProvider sp, ILogger logger)
    {
        var ordemService = sp.GetRequiredService<IOrdemServicoService>();

        var cliente = (await sp.GetRequiredService<IClienteService>().ObterTodosAsync())
            .FirstOrDefault(c => c.Documento == DocumentoDemo);
        var servico = (await sp.GetRequiredService<IServicoService>().ObterTodosAsync())
            .FirstOrDefault(s => s.Descricao == ServicoDemo);
        var catalogo = (await sp.GetRequiredService<ICatalogoService>().ObterTodosAsync())
            .FirstOrDefault(c => c.Nome == CatalogoDemo);

        if (cliente is null || servico is null || catalogo is null)
            return;

        var ordem = (await ordemService.ObterTodosAsync()).FirstOrDefault(o => o.ClienteId == cliente.Id);

        if (ordem is null || ordem.Orcamento is not null)
            return;

        if (!ordem.ItensServico.Any(i => i.ServicoId == servico.Id)
            && ordem.Status is EnumsOrdemServico.StatusOrdemServico.Recebida
                            or EnumsOrdemServico.StatusOrdemServico.EmDiagnostico)
        {
            ordem = await ordemService.AdicionarServicoAsync(
                ordem.Id, new AdicionarServicoOrdemInputDto { ServicoId = servico.Id });

            logger.LogInformation("Demonstração: serviço {ServicoId} incluído na OS {OrdemId}.", servico.Id, ordem.Id);
        }

        if (ordem.ItensPecaInsumo.Any(i => i.CatalogoId == catalogo.Id))
            return;

        // Peças e insumos só podem ser incluídos com a OS em diagnóstico
        if (ordem.Status == EnumsOrdemServico.StatusOrdemServico.Recebida)
            ordem = await ordemService.IniciarDiagnosticoAsync(ordem.Id);

        if (ordem.Status != EnumsOrdemServico.StatusOrdemServico.EmDiagnostico)
            return;

        await ordemService.AdicionarPecaInsumoAsync(
            ordem.Id, new AdicionarPecaInsumoOrdemInputDto { CatalogoId = catalogo.Id, Quantidade = 2 });

        logger.LogInformation("Demonstração: 2 un. do catálogo {CatalogoId} incluídas na OS {OrdemId}.", catalogo.Id, ordem.Id);
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
