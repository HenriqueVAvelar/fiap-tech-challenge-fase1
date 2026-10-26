using System.Net;
using Oficina.Api.IntegrationTests.Support;
using Oficina.Application.DTOs;
using Oficina.Application.DTOs.Indicadores;
using Oficina.Application.DTOs.OrdemServico;
using static Oficina.Domain.Enums.EnumsOrcamento;
using static Oficina.Domain.Enums.EnumsOrdemServico;

namespace Oficina.Api.IntegrationTests;

[Collection(ApiCollection.Nome)]
public class OrdemServicoIntegrationTests : IntegrationTestBase
{
    public OrdemServicoIntegrationTests(OficinaApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task FluxoCompleto_DaRecepcaoAteAEntrega()
    {
        var admin = await ClienteAdminAsync();
        var mecanico = await ClienteMecanicoAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var servico = await CriarServicoAsync(admin, 150m);
        var (catalogo, estoque) = await CriarPecaComEstoqueAsync(admin, 40m, 10);

        // Criação: o cliente é identificado pelo CPF (aqui com máscara) e o veículo pela placa
        var criacao = await admin.PostJsonAsync("/OrdensServico", new CriarOrdemServicoInputDto
        {
            Documento = DadosDeTeste.FormatarCpf(cliente.Documento),
            Placa = veiculo.Placa
        });

        await criacao.DeveTerStatusAsync(HttpStatusCode.Created);
        criacao.DeveSerJson();
        Assert.EndsWith($"/OrdensServico/{cliente.Documento}", criacao.Headers.Location?.ToString());

        var ordem = await criacao.LerAsync<OrdemServicoDto>();
        Assert.True(ordem.Id > 0);
        Assert.Equal(cliente.Id, ordem.ClienteId);
        Assert.Equal(veiculo.Id, ordem.VeiculoId);
        Assert.Equal(StatusOrdemServico.Recebida, ordem.Status);
        Assert.Null(ordem.Orcamento);

        // Diagnóstico
        ordem = await PostOrdemAsync(mecanico, $"/OrdensServico/{ordem.Id}/iniciar-diagnostico");
        Assert.Equal(StatusOrdemServico.EmDiagnostico, ordem.Status);
        Assert.NotNull(ordem.DataInicioDiagnostico);

        // Serviços e peças/insumos
        ordem = await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/servicos", new AdicionarServicoOrdemInputDto { ServicoId = servico.Id });
        var itemServico = Assert.Single(ordem.ItensServico);
        Assert.Equal(servico.Id, itemServico.ServicoId);
        Assert.Equal(150m, itemServico.Valor);

        ordem = await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/pecas-insumos",
            new AdicionarPecaInsumoOrdemInputDto { CatalogoId = catalogo.Id, Quantidade = 3 });
        var itemPeca = Assert.Single(ordem.ItensPecaInsumo);
        Assert.Equal(3, itemPeca.Quantidade);
        Assert.Equal(40m, itemPeca.ValorUnitario);
        Assert.Equal(120m, itemPeca.ValorTotal);
        Assert.False(itemPeca.Consumido);

        // Incluir a peça na OS apenas reserva a intenção: o estoque só baixa quando o consumo é registrado
        Assert.Equal(10, await SaldoAsync(admin, estoque.Id));

        // Orçamento gerado automaticamente a partir dos serviços e das peças
        ordem = await PostOrdemAsync(mecanico, $"/OrdensServico/{ordem.Id}/concluir-diagnostico");
        Assert.NotNull(ordem.DataConclusaoDiagnostico);

        ordem = await PostOrdemAsync(mecanico, $"/OrdensServico/{ordem.Id}/gerar-orcamento");
        Assert.NotNull(ordem.Orcamento);
        Assert.Equal(150m, ordem.Orcamento.ValorServicos);
        Assert.Equal(120m, ordem.Orcamento.ValorPecasInsumos);
        Assert.Equal(270m, ordem.Orcamento.ValorTotal);
        Assert.Equal(StatusOrcamento.Pendente, ordem.Orcamento.Status);

        // Envio do orçamento ao cliente
        ordem = await PostOrdemAsync(mecanico, $"/OrdensServico/{ordem.Id}/disponibilizar-orcamento");
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, ordem.Status);
        Assert.NotNull(ordem.Orcamento!.DataDisponibilizacao);

        // O cliente acompanha a OS pela API, sem autenticação administrativa
        var anonimo = ClienteAnonimo();
        var acompanhamento = await (await anonimo.GetAsync($"/OrdensServico/{cliente.Documento}"))
            .DeveTerStatusAsync(HttpStatusCode.OK);
        acompanhamento.DeveSerJson();

        var ordensDoCliente = await acompanhamento.LerAsync<List<OrdemServicoDto>>();
        var acompanhada = Assert.Single(ordensDoCliente);
        Assert.Equal(ordem.Id, acompanhada.Id);
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, acompanhada.Status);
        Assert.Equal(270m, acompanhada.Orcamento!.ValorTotal);

        // O cliente aprova o orçamento, identificando-se pelo documento
        var aprovacao = await anonimo.PostAsync($"/OrdensServico/{cliente.Documento}/aprovar-orcamento", null);
        await aprovacao.DeveTerStatusAsync(HttpStatusCode.OK);
        aprovacao.DeveSerJson();

        ordem = await aprovacao.LerAsync<OrdemServicoDto>();
        Assert.Equal(StatusOrcamento.Aprovado, ordem.Orcamento!.Status);
        Assert.NotNull(ordem.Orcamento.DataDecisao);

        // Execução
        ordem = await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/iniciar-execucao");
        Assert.Equal(StatusOrdemServico.EmExecucao, ordem.Status);
        Assert.NotNull(ordem.DataInicioExecucao);

        // Consumo das peças: marca o item como consumido e baixa o estoque (10 - 3)
        ordem = await PostOrdemAsync(mecanico, $"/OrdensServico/{ordem.Id}/consumir-pecas-insumos",
            new RegistrarConsumoOrdemInputDto { ItensIds = new List<int> { itemPeca.Id } });
        Assert.True(Assert.Single(ordem.ItensPecaInsumo).Consumido);
        Assert.Equal(7, await SaldoAsync(admin, estoque.Id));

        // Finalização e entrega
        ordem = await PostOrdemAsync(mecanico, $"/OrdensServico/{ordem.Id}/finalizar");
        Assert.Equal(StatusOrdemServico.Finalizada, ordem.Status);
        Assert.NotNull(ordem.DataFinalizacao);

        ordem = await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/entregar");
        Assert.Equal(StatusOrdemServico.Entregue, ordem.Status);
        Assert.NotNull(ordem.DataEntrega);

        // A consulta do cliente reflete o status final
        var final = await (await anonimo.GetAsync($"/OrdensServico/{cliente.Documento}")).LerAsync<List<OrdemServicoDto>>();
        Assert.Equal(StatusOrdemServico.Entregue, Assert.Single(final).Status);

        // A OS aparece na listagem administrativa
        var listagem = await (await admin.GetAsync("/OrdensServico")).DeveTerStatusAsync(HttpStatusCode.OK);
        Assert.Contains(await listagem.LerAsync<List<OrdemServicoDto>>(), o => o.Id == ordem.Id);

        // E entra no cálculo do tempo médio de execução
        var indicador = await (await admin.GetAsync("/Indicadores/tempo-medio-execucao")).DeveTerStatusAsync(HttpStatusCode.OK);
        indicador.DeveSerJson();

        var tempoMedio = await indicador.LerAsync<TempoMedioExecucaoDto>();
        Assert.True(tempoMedio.QuantidadeOrdensConsideradas >= 1);
        Assert.NotNull(tempoMedio.TempoMedioMinutos);
        Assert.True(tempoMedio.TempoMedioMinutos >= 0);
    }

    [Fact]
    public async Task RejeitarOrcamento_ImpedeAExecucao()
    {
        var admin = await ClienteAdminAsync();
        var (ordem, cliente, veiculo) = await CriarOrdemComOrcamentoDisponibilizadoAsync(admin);

        var rejeicao = await ClienteAnonimo().PostAsync(
            $"/OrdensServico/{cliente.Documento}/rejeitar-orcamento?placa={veiculo.Placa}", null);

        await rejeicao.DeveTerStatusAsync(HttpStatusCode.OK);

        var rejeitada = await rejeicao.LerAsync<OrdemServicoDto>();
        Assert.Equal(StatusOrcamento.Rejeitado, rejeitada.Orcamento!.Status);
        Assert.NotNull(rejeitada.Orcamento.DataDecisao);

        // Sem aprovação, a execução não pode começar
        var execucao = await admin.PostAsync($"/OrdensServico/{ordem.Id}/iniciar-execucao", null);
        await execucao.DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AprovarOrcamento_ComDuasOrdensAguardando_ExigeAPlaca()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var primeiroVeiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var segundoVeiculo = await CriarVeiculoAsync(admin, cliente.Id);

        await PrepararOrdemAguardandoAsync(admin, cliente, primeiroVeiculo);
        await PrepararOrdemAguardandoAsync(admin, cliente, segundoVeiculo);

        var anonimo = ClienteAnonimo();

        // Sem a placa não há como saber qual das duas ordens o cliente quer aprovar
        await (await anonimo.PostAsync($"/OrdensServico/{cliente.Documento}/aprovar-orcamento", null))
            .DeveTerStatusAsync(HttpStatusCode.Conflict);

        var aprovacao = await anonimo.PostAsync(
            $"/OrdensServico/{cliente.Documento}/aprovar-orcamento?placa={segundoVeiculo.Placa}", null);

        await aprovacao.DeveTerStatusAsync(HttpStatusCode.OK);
        Assert.Equal(segundoVeiculo.Id, (await aprovacao.LerAsync<OrdemServicoDto>()).VeiculoId);
    }

    [Fact]
    public async Task Criar_ComClienteInexistente_Retorna404()
    {
        var admin = await ClienteAdminAsync();

        var resposta = await admin.PostJsonAsync("/OrdensServico", new CriarOrdemServicoInputDto
        {
            Documento = DadosDeTeste.Cpf(),
            Placa = DadosDeTeste.Placa()
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Criar_ComVeiculoInexistente_Retorna404()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);

        var resposta = await admin.PostJsonAsync("/OrdensServico", new CriarOrdemServicoInputDto
        {
            Documento = cliente.Documento,
            Placa = DadosDeTeste.Placa()
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Criar_ComVeiculoDeOutroCliente_Retorna409()
    {
        var admin = await ClienteAdminAsync();
        var dono = await CriarClienteAsync(admin);
        var outro = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, dono.Id);

        var resposta = await admin.PostJsonAsync("/OrdensServico", new CriarOrdemServicoInputDto
        {
            Documento = outro.Documento,
            Placa = veiculo.Placa
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("123.456.789-00", "ABC1D23")]
    [InlineData("529.982.247-25", "123")]
    public async Task Criar_ComDocumentoOuPlacaInvalidos_Retorna400(string documento, string placa)
    {
        var admin = await ClienteAdminAsync();

        var resposta = await admin.PostJsonAsync("/OrdensServico", new CriarOrdemServicoInputDto
        {
            Documento = documento,
            Placa = placa
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Criar_SemToken_Retorna401()
    {
        var resposta = await ClienteAnonimo().PostJsonAsync("/OrdensServico", new CriarOrdemServicoInputDto
        {
            Documento = DadosDeTeste.Cpf(),
            Placa = DadosDeTeste.Placa()
        });

        await resposta.DeveTerStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Mecanico_NaoPodeCriarNemEntregarOrdem()
    {
        var admin = await ClienteAdminAsync();
        var mecanico = await ClienteMecanicoAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var ordem = await CriarOrdemAsync(admin, cliente, veiculo);

        var criacao = await mecanico.PostJsonAsync("/OrdensServico", new CriarOrdemServicoInputDto
        {
            Documento = cliente.Documento,
            Placa = veiculo.Placa
        });
        await criacao.DeveTerStatusAsync(HttpStatusCode.Forbidden);

        await (await mecanico.PostAsync($"/OrdensServico/{ordem.Id}/entregar", null))
            .DeveTerStatusAsync(HttpStatusCode.Forbidden);

        await (await mecanico.GetAsync("/OrdensServico")).DeveTerStatusAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Consulta_DeClienteInexistente_Retorna404()
    {
        var resposta = await ClienteAnonimo().GetAsync($"/OrdensServico/{DadosDeTeste.Cpf()}");

        await resposta.DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Consulta_DeClienteSemOrdens_RetornaListaVazia()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);

        var resposta = await ClienteAnonimo().GetAsync($"/OrdensServico/{cliente.Documento}");

        await resposta.DeveTerStatusAsync(HttpStatusCode.OK);
        Assert.Empty(await resposta.LerAsync<List<OrdemServicoDto>>());
    }

    [Fact]
    public async Task Transicoes_ForaDaOrdem_Retornam409()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var ordem = await CriarOrdemAsync(admin, cliente, veiculo);

        // Uma OS recém-recebida não pode pular etapas
        foreach (var acao in new[] { "concluir-diagnostico", "gerar-orcamento", "disponibilizar-orcamento", "iniciar-execucao", "finalizar", "entregar" })
            await (await admin.PostAsync($"/OrdensServico/{ordem.Id}/{acao}", null)).DeveTerStatusAsync(HttpStatusCode.Conflict);

        // Nenhuma das tentativas alterou o status
        var consulta = await (await admin.GetAsync($"/OrdensServico/{cliente.Documento}")).LerAsync<List<OrdemServicoDto>>();
        Assert.Equal(StatusOrdemServico.Recebida, Assert.Single(consulta).Status);
    }

    [Fact]
    public async Task IniciarDiagnostico_DuasVezes_Retorna409()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var ordem = await CriarOrdemAsync(admin, cliente, veiculo);

        await (await admin.PostAsync($"/OrdensServico/{ordem.Id}/iniciar-diagnostico", null)).DeveTerStatusAsync(HttpStatusCode.OK);
        await (await admin.PostAsync($"/OrdensServico/{ordem.Id}/iniciar-diagnostico", null)).DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GerarOrcamento_SemServicos_Retorna409()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var ordem = await CriarOrdemAsync(admin, cliente, veiculo);

        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/iniciar-diagnostico");
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/concluir-diagnostico");

        await (await admin.PostAsync($"/OrdensServico/{ordem.Id}/gerar-orcamento", null)).DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GerarOrcamento_ComDiagnosticoEmAndamento_Retorna409()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var servico = await CriarServicoAsync(admin, 80m);
        var ordem = await CriarOrdemAsync(admin, cliente, veiculo);

        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/iniciar-diagnostico");
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/servicos", new AdicionarServicoOrdemInputDto { ServicoId = servico.Id });

        await (await admin.PostAsync($"/OrdensServico/{ordem.Id}/gerar-orcamento", null)).DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AdicionarServico_AposGerarOrcamento_Retorna409()
    {
        var admin = await ClienteAdminAsync();
        var (ordem, _, _) = await CriarOrdemComOrcamentoDisponibilizadoAsync(admin);
        var outroServico = await CriarServicoAsync(admin, 10m);

        var resposta = await admin.PostJsonAsync($"/OrdensServico/{ordem.Id}/servicos", new AdicionarServicoOrdemInputDto { ServicoId = outroServico.Id });

        await resposta.DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AdicionarServico_Inativo_Retorna409()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var inativo = await CriarServicoAsync(admin, 50m, ativo: false);
        var ordem = await CriarOrdemAsync(admin, cliente, veiculo);

        var resposta = await admin.PostJsonAsync($"/OrdensServico/{ordem.Id}/servicos", new AdicionarServicoOrdemInputDto { ServicoId = inativo.Id });

        await resposta.DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AdicionarServico_Inexistente_Retorna404()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var ordem = await CriarOrdemAsync(admin, cliente, veiculo);

        var resposta = await admin.PostJsonAsync($"/OrdensServico/{ordem.Id}/servicos", new AdicionarServicoOrdemInputDto { ServicoId = int.MaxValue });

        await resposta.DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Ordem_Inexistente_Retorna404()
    {
        var admin = await ClienteAdminAsync();

        var resposta = await admin.PostAsync($"/OrdensServico/{int.MaxValue}/iniciar-diagnostico", null);

        await resposta.DeveTerStatusAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AdicionarPecaInsumo_ComEstoqueInsuficiente_Retorna409()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var (catalogo, _) = await CriarPecaComEstoqueAsync(admin, 25m, 2);
        var ordem = await CriarOrdemAsync(admin, cliente, veiculo);

        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/iniciar-diagnostico");

        var resposta = await admin.PostJsonAsync($"/OrdensServico/{ordem.Id}/pecas-insumos",
            new AdicionarPecaInsumoOrdemInputDto { CatalogoId = catalogo.Id, Quantidade = 3 });

        await resposta.DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AdicionarPecaInsumo_SemEstoqueCadastrado_Retorna409()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var semEstoque = await CriarCatalogoAsync(admin, 25m);
        var ordem = await CriarOrdemAsync(admin, cliente, veiculo);

        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/iniciar-diagnostico");

        var resposta = await admin.PostJsonAsync($"/OrdensServico/{ordem.Id}/pecas-insumos",
            new AdicionarPecaInsumoOrdemInputDto { CatalogoId = semEstoque.Id, Quantidade = 1 });

        await resposta.DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AdicionarPecaInsumo_ForaDoDiagnostico_Retorna409()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var (catalogo, _) = await CriarPecaComEstoqueAsync(admin, 25m, 5);
        var ordem = await CriarOrdemAsync(admin, cliente, veiculo);

        // A OS ainda está Recebida
        var resposta = await admin.PostJsonAsync($"/OrdensServico/{ordem.Id}/pecas-insumos",
            new AdicionarPecaInsumoOrdemInputDto { CatalogoId = catalogo.Id, Quantidade = 1 });

        await resposta.DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AdicionarPecaInsumo_ComQuantidadeZero_Retorna400()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var (catalogo, _) = await CriarPecaComEstoqueAsync(admin, 25m, 5);
        var ordem = await CriarOrdemAsync(admin, cliente, veiculo);

        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/iniciar-diagnostico");

        var resposta = await admin.PostJsonAsync($"/OrdensServico/{ordem.Id}/pecas-insumos",
            new AdicionarPecaInsumoOrdemInputDto { CatalogoId = catalogo.Id, Quantidade = 0 });

        await resposta.DeveTerStatusAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegistrarConsumo_ComEstoqueInsuficienteEmUmItem_NaoAlteraNada()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var servico = await CriarServicoAsync(admin, 100m);
        var (catalogoA, estoqueA) = await CriarPecaComEstoqueAsync(admin, 10m, 5);
        var (catalogoB, estoqueB) = await CriarPecaComEstoqueAsync(admin, 20m, 5);

        var ordem = await CriarOrdemAsync(admin, cliente, veiculo);
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/iniciar-diagnostico");
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/servicos", new AdicionarServicoOrdemInputDto { ServicoId = servico.Id });
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/pecas-insumos", new AdicionarPecaInsumoOrdemInputDto { CatalogoId = catalogoA.Id, Quantidade = 2 });
        ordem = await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/pecas-insumos", new AdicionarPecaInsumoOrdemInputDto { CatalogoId = catalogoB.Id, Quantidade = 4 });
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/concluir-diagnostico");
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/gerar-orcamento");
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/disponibilizar-orcamento");
        await (await ClienteAnonimo().PostAsync($"/OrdensServico/{cliente.Documento}/aprovar-orcamento", null)).DeveTerStatusAsync(HttpStatusCode.OK);
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/iniciar-execucao");

        // Entre a inclusão na OS e o consumo, o estoque do segundo item cai abaixo do necessário
        await (await admin.PutJsonAsync($"/Estoques/{estoqueB.Id}", new EstoqueUpdateInputDto { Quantidade = 1 }))
            .DeveTerStatusAsync(HttpStatusCode.NoContent);

        var itensIds = ordem.ItensPecaInsumo.Select(i => i.Id).ToList();
        var consumo = await admin.PostJsonAsync($"/OrdensServico/{ordem.Id}/consumir-pecas-insumos",
            new RegistrarConsumoOrdemInputDto { ItensIds = itensIds });

        await consumo.DeveTerStatusAsync(HttpStatusCode.Conflict);

        // Tudo ou nada: o primeiro item também não foi baixado nem marcado como consumido
        Assert.Equal(5, await SaldoAsync(admin, estoqueA.Id));
        Assert.Equal(1, await SaldoAsync(admin, estoqueB.Id));

        var apos = (await (await admin.GetAsync("/OrdensServico")).LerAsync<List<OrdemServicoDto>>()).Single(o => o.Id == ordem.Id);
        Assert.All(apos.ItensPecaInsumo, item => Assert.False(item.Consumido));
    }

    [Fact]
    public async Task RegistrarConsumo_DoMesmoItemDuasVezes_Retorna409ESaldoBaixaUmaVez()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var servico = await CriarServicoAsync(admin, 100m);
        var (catalogo, estoque) = await CriarPecaComEstoqueAsync(admin, 10m, 5);

        var ordem = await CriarOrdemAsync(admin, cliente, veiculo);
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/iniciar-diagnostico");
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/servicos", new AdicionarServicoOrdemInputDto { ServicoId = servico.Id });
        ordem = await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/pecas-insumos", new AdicionarPecaInsumoOrdemInputDto { CatalogoId = catalogo.Id, Quantidade = 2 });
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/concluir-diagnostico");
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/gerar-orcamento");
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/disponibilizar-orcamento");
        await (await ClienteAnonimo().PostAsync($"/OrdensServico/{cliente.Documento}/aprovar-orcamento", null)).DeveTerStatusAsync(HttpStatusCode.OK);
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/iniciar-execucao");

        var consumo = new RegistrarConsumoOrdemInputDto { ItensIds = new List<int> { ordem.ItensPecaInsumo.Single().Id } };

        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/consumir-pecas-insumos", consumo);
        await (await admin.PostJsonAsync($"/OrdensServico/{ordem.Id}/consumir-pecas-insumos", consumo))
            .DeveTerStatusAsync(HttpStatusCode.Conflict);

        Assert.Equal(3, await SaldoAsync(admin, estoque.Id));
    }

    [Fact]
    public async Task RegistrarConsumo_SemItens_Retorna400()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);
        var ordem = await CriarOrdemAsync(admin, cliente, veiculo);

        var resposta = await admin.PostJsonAsync($"/OrdensServico/{ordem.Id}/consumir-pecas-insumos",
            new RegistrarConsumoOrdemInputDto { ItensIds = new List<int>() });

        await resposta.DeveTerStatusAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AprovarOrcamento_SemOrdemAguardandoDecisao_Retorna409()
    {
        var admin = await ClienteAdminAsync();
        var cliente = await CriarClienteAsync(admin);

        var resposta = await ClienteAnonimo().PostAsync($"/OrdensServico/{cliente.Documento}/aprovar-orcamento", null);

        await resposta.DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AprovarOrcamento_ComDocumentoInvalido_Retorna400()
    {
        var resposta = await ClienteAnonimo().PostAsync("/OrdensServico/123.456.789-00/aprovar-orcamento", null);

        await resposta.DeveTerStatusAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AprovarOrcamento_DuasVezes_Retorna409()
    {
        var admin = await ClienteAdminAsync();
        var (_, cliente, _) = await CriarOrdemComOrcamentoDisponibilizadoAsync(admin);
        var anonimo = ClienteAnonimo();

        await (await anonimo.PostAsync($"/OrdensServico/{cliente.Documento}/aprovar-orcamento", null)).DeveTerStatusAsync(HttpStatusCode.OK);

        // Depois da decisão não existe mais OS aguardando o cliente
        await (await anonimo.PostAsync($"/OrdensServico/{cliente.Documento}/aprovar-orcamento", null)).DeveTerStatusAsync(HttpStatusCode.Conflict);
    }

    private static async Task<OrdemServicoDto> CriarOrdemAsync(HttpClient admin, ClienteDto cliente, VeiculoDto veiculo)
    {
        var resposta = await (await admin.PostJsonAsync("/OrdensServico", new CriarOrdemServicoInputDto
        {
            Documento = cliente.Documento,
            Placa = veiculo.Placa
        })).DeveTerStatusAsync(HttpStatusCode.Created);

        return await resposta.LerAsync<OrdemServicoDto>();
    }

    private async Task<(OrdemServicoDto Ordem, ClienteDto Cliente, VeiculoDto Veiculo)> CriarOrdemComOrcamentoDisponibilizadoAsync(
        HttpClient admin)
    {
        var cliente = await CriarClienteAsync(admin);
        var veiculo = await CriarVeiculoAsync(admin, cliente.Id);

        return (await PrepararOrdemAguardandoAsync(admin, cliente, veiculo), cliente, veiculo);
    }

    // Leva uma OS até "Aguardando aprovação", com um serviço e uma peça no orçamento
    private async Task<OrdemServicoDto> PrepararOrdemAguardandoAsync(HttpClient admin, ClienteDto cliente, VeiculoDto veiculo)
    {
        var servico = await CriarServicoAsync(admin, 200m);
        var (catalogo, _) = await CriarPecaComEstoqueAsync(admin, 30m, 10);

        var ordem = await CriarOrdemAsync(admin, cliente, veiculo);

        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/iniciar-diagnostico");
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/servicos", new AdicionarServicoOrdemInputDto { ServicoId = servico.Id });
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/pecas-insumos", new AdicionarPecaInsumoOrdemInputDto { CatalogoId = catalogo.Id, Quantidade = 2 });
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/concluir-diagnostico");
        await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/gerar-orcamento");

        return await PostOrdemAsync(admin, $"/OrdensServico/{ordem.Id}/disponibilizar-orcamento");
    }

    private static async Task<OrdemServicoDto> PostOrdemAsync(HttpClient client, string rota)
    {
        var resposta = await (await client.PostAsync(rota, null)).DeveTerStatusAsync(HttpStatusCode.OK);

        return await resposta.LerAsync<OrdemServicoDto>();
    }

    private static async Task<OrdemServicoDto> PostOrdemAsync<T>(HttpClient client, string rota, T corpo)
    {
        var resposta = await (await client.PostJsonAsync(rota, corpo)).DeveTerStatusAsync(HttpStatusCode.OK);

        return await resposta.LerAsync<OrdemServicoDto>();
    }

    private static async Task<int> SaldoAsync(HttpClient client, int estoqueId)
    {
        var resposta = await (await client.GetAsync($"/Estoques/{estoqueId}")).DeveTerStatusAsync(HttpStatusCode.OK);

        return (await resposta.LerAsync<EstoqueDto>()).Quantidade;
    }
}
