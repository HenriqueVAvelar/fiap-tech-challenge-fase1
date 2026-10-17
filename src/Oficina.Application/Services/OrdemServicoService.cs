using Oficina.Application.DTOs.OrdemServico;
using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;

namespace Oficina.Application.Services;

public class OrdemServicoService : IOrdemServicoService
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly ICatalogoRepository _catalogoRepository;
    private readonly IEstoqueRepository _estoqueRepository;

    public OrdemServicoService(
        IOrdemServicoRepository ordemServicoRepository,
        IClienteRepository clienteRepository,
        IVeiculoRepository veiculoRepository,
        IServicoRepository servicoRepository,
        ICatalogoRepository catalogoRepository,
        IEstoqueRepository estoqueRepository)
    {
        _ordemServicoRepository = ordemServicoRepository;
        _clienteRepository = clienteRepository;
        _veiculoRepository = veiculoRepository;
        _servicoRepository = servicoRepository;
        _catalogoRepository = catalogoRepository;
        _estoqueRepository = estoqueRepository;
    }

    public async Task<IEnumerable<OrdemServicoDto>> ObterTodosAsync()
    {
        var ordens = await _ordemServicoRepository.ObterTodosAsync();

        return ordens.Select(Mapear);
    }

    public async Task<OrdemServicoDto?> ObterPorIdAsync(int id)
    {
        var ordem = await _ordemServicoRepository.ObterPorIdAsync(id);

        return ordem is null
            ? null
            : Mapear(ordem);
    }

    public async Task<OrdemServicoDto> CriarAsync(
        CriarOrdemServicoInputDto input)
    {
        var cliente =
            await _clienteRepository.ObterPorIdAsync(input.ClienteId);

        if (cliente is null)
            throw new KeyNotFoundException(
                $"Cliente {input.ClienteId} não encontrado.");

        var veiculo =
            await _veiculoRepository.ObterPorIdAsync(input.VeiculoId);

        if (veiculo is null)
            throw new KeyNotFoundException(
                $"Veículo {input.VeiculoId} não encontrado.");

        if (veiculo.ClienteId != cliente.Id)
        {
            throw new InvalidOperationException(
                "O veículo informado não pertence ao cliente da Ordem de Serviço.");
        }

        var ordemServico =
            new Domain.Entities.OrdemServico(
                cliente.Id,
                veiculo.Id);

        await _ordemServicoRepository.AdicionarAsync(
            ordemServico);

        return Mapear(ordemServico);
    }

    public async Task<OrdemServicoDto> AdicionarServicoAsync(
        int ordemServicoId,
        AdicionarServicoOrdemInputDto input)
    {
        var ordem = await ObterOrdemAsync(ordemServicoId);

        var servico =
            await _servicoRepository.ObterPorIdAsync(input.ServicoId);

        if (servico is null)
            throw new KeyNotFoundException(
                $"Serviço {input.ServicoId} não encontrado.");

        if (!servico.Ativo)
            throw new InvalidOperationException(
                "Não é possível adicionar um serviço inativo à Ordem de Serviço.");

        ordem.AdicionarServico(
            servico.Id,
            servico.Descricao,
            servico.Valor);

        await _ordemServicoRepository.AtualizarAsync(ordem);

        return Mapear(ordem);
    }

    public async Task<OrdemServicoDto> IniciarDiagnosticoAsync(
        int ordemServicoId)
    {
        var ordem = await ObterOrdemAsync(ordemServicoId);

        ordem.IniciarDiagnostico();

        await _ordemServicoRepository.AtualizarAsync(ordem);

        return Mapear(ordem);
    }

    public async Task<OrdemServicoDto> AdicionarPecaInsumoAsync(
        int ordemServicoId,
        AdicionarPecaInsumoOrdemInputDto input)
    {
        var ordem = await ObterOrdemAsync(ordemServicoId);

        var item =
            await _catalogoRepository.ObterPorIdAsync(input.CatalogoId);

        if (item is null)
            throw new KeyNotFoundException(
                $"Item de catálogo {input.CatalogoId} não encontrado.");

        if (!item.Ativo)
            throw new InvalidOperationException(
                "Não é possível adicionar um item de catálogo inativo à Ordem de Serviço.");

        await GarantirSaldoAsync(ordem, item, input.Quantidade);

        ordem.AdicionarPecaInsumo(
            item.Id,
            item.Nome,
            item.Tipo,
            input.Quantidade,
            item.Preco);

        await _ordemServicoRepository.AtualizarAsync(ordem);

        return Mapear(ordem);
    }

    public async Task<OrdemServicoDto> RegistrarConsumoAsync(
        int ordemServicoId,
        RegistrarConsumoOrdemInputDto input)
    {
        var ordem = await ObterOrdemAsync(ordemServicoId);

        var itens = ordem.RegistrarConsumoPecasInsumos(input.ItensIds);

        foreach (var item in itens)
        {
            var estoque =
                await _estoqueRepository.ObterPorCatalogoIdAsync(item.CatalogoId);

            if (estoque is null)
                throw new InvalidOperationException(
                    $"Não há estoque cadastrado para '{item.Nome}'.");

            if (!estoque.PossuiSaldoPara(item.Quantidade))
                throw new InvalidOperationException(
                    $"Estoque insuficiente para '{item.Nome}'. " +
                    $"Saldo: {estoque.Quantidade}; necessário: {item.Quantidade}.");

            estoque.Consumir(item.Quantidade);
        }
        
        await _ordemServicoRepository.AtualizarAsync(ordem);

        return Mapear(ordem);
    }

    public async Task<OrdemServicoDto> ConcluirDiagnosticoAsync(
        int ordemServicoId)
    {
        var ordem = await ObterOrdemAsync(ordemServicoId);

        ordem.ConcluirDiagnostico();

        await _ordemServicoRepository.AtualizarAsync(ordem);

        return Mapear(ordem);
    }

    public async Task<OrdemServicoDto> GerarOrcamentoAsync(
        int ordemServicoId)
    {
        var ordem = await ObterOrdemAsync(ordemServicoId);

        ordem.GerarOrcamento();

        await _ordemServicoRepository.AtualizarAsync(ordem);

        return Mapear(ordem);
    }

    public async Task<OrdemServicoDto> DisponibilizarOrcamentoAsync(
        int ordemServicoId)
    {
        var ordem = await ObterOrdemAsync(ordemServicoId);

        ordem.DisponibilizarOrcamento();

        await _ordemServicoRepository.AtualizarAsync(ordem);

        return Mapear(ordem);
    }

    public async Task<OrdemServicoDto> AprovarOrcamentoAsync(
        int ordemServicoId)
    {
        var ordem = await ObterOrdemAsync(ordemServicoId);

        ordem.AprovarOrcamento();

        await _ordemServicoRepository.AtualizarAsync(ordem);

        return Mapear(ordem);
    }

    public async Task<OrdemServicoDto> RejeitarOrcamentoAsync(
        int ordemServicoId)
    {
        var ordem = await ObterOrdemAsync(ordemServicoId);

        ordem.RejeitarOrcamento();

        await _ordemServicoRepository.AtualizarAsync(ordem);

        return Mapear(ordem);
    }

    public async Task<OrdemServicoDto> IniciarExecucaoAsync(
        int ordemServicoId)
    {
        var ordem = await ObterOrdemAsync(ordemServicoId);

        ordem.IniciarExecucao();

        await _ordemServicoRepository.AtualizarAsync(ordem);

        return Mapear(ordem);
    }

    public async Task<OrdemServicoDto> FinalizarAsync(
        int ordemServicoId)
    {
        var ordem = await ObterOrdemAsync(ordemServicoId);

        ordem.Finalizar();

        await _ordemServicoRepository.AtualizarAsync(ordem);

        return Mapear(ordem);
    }

    public async Task<OrdemServicoDto> EntregarAsync(
        int ordemServicoId)
    {
        var ordem = await ObterOrdemAsync(ordemServicoId);

        ordem.Entregar();

        await _ordemServicoRepository.AtualizarAsync(ordem);

        return Mapear(ordem);
    }
    
    private async Task GarantirSaldoAsync(
        Domain.Entities.OrdemServico ordem,
        Catalogo item,
        int quantidade)
    {
        var estoque =
            await _estoqueRepository.ObterPorCatalogoIdAsync(item.Id);

        var jaIncluida = ordem.ItensPecaInsumo
            .Where(i => i.CatalogoId == item.Id && !i.Consumido)
            .Sum(i => i.Quantidade);

        var saldo = estoque?.Quantidade ?? 0;

        if (estoque is null || !estoque.PossuiSaldoPara(jaIncluida + quantidade))
            throw new InvalidOperationException(
                $"Estoque insuficiente para '{item.Nome}'. " +
                $"Saldo: {saldo}; já incluído na OS: {jaIncluida}; solicitado: {quantidade}.");
    }

    private async Task<Domain.Entities.OrdemServico> ObterOrdemAsync(
        int ordemServicoId)
    {
        var ordem =
            await _ordemServicoRepository.ObterPorIdAsync(
                ordemServicoId);

        if (ordem is null)
        {
            throw new KeyNotFoundException(
                $"Ordem de Serviço {ordemServicoId} não encontrada.");
        }

        return ordem;
    }

    private static OrdemServicoDto Mapear(
        Domain.Entities.OrdemServico ordem)
    {
        return new OrdemServicoDto
        {
            Id = ordem.Id,
            ClienteId = ordem.ClienteId,
            VeiculoId = ordem.VeiculoId,
            Status = ordem.Status,
            DataCriacao = ordem.DataCriacao,
            DataInicioDiagnostico = ordem.DataInicioDiagnostico,
            DataConclusaoDiagnostico = ordem.DataConclusaoDiagnostico,
            DataInicioExecucao = ordem.DataInicioExecucao,
            DataFinalizacao = ordem.DataFinalizacao,
            DataEntrega = ordem.DataEntrega,

            ItensServico = ordem.ItensServico.Select(
                item => new ItemServicoDto
                {
                    ServicoId = item.ServicoId,
                    Descricao = item.Descricao,
                    Valor = item.Valor
                }),

            ItensPecaInsumo = ordem.ItensPecaInsumo.Select(
                item => new ItemPecaInsumoDto
                {
                    Id = item.Id,
                    CatalogoId = item.CatalogoId,
                    Consumido = item.Consumido,
                    Nome = item.Nome,
                    Tipo = item.Tipo,
                    Quantidade = item.Quantidade,
                    ValorUnitario = item.ValorUnitario,
                    ValorTotal = item.ValorTotal
                }),

            Orcamento = ordem.Orcamento is null
                ? null
                : new OrcamentoDto
                {
                    ValorServicos =
                        ordem.Orcamento.ValorServicos,

                    ValorPecasInsumos =
                        ordem.Orcamento.ValorPecasInsumos,

                    ValorTotal =
                        ordem.Orcamento.ValorTotal,

                    Status =
                        ordem.Orcamento.Status,

                    DataGeracao =
                        ordem.Orcamento.DataGeracao,

                    DataDisponibilizacao =
                        ordem.Orcamento.DataDisponibilizacao,

                    DataDecisao =
                        ordem.Orcamento.DataDecisao
                }
        };
    }
}