using Oficina.Application.DTOs.Auth;
using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;

namespace Oficina.Infrastructure.UnitTests.Seed;

// Repositórios em memória usados pelos testes do seeder; os serviços da aplicação são os reais.
internal sealed class RepositoriosEmMemoria
{
    public CatalogoEmMemoria Catalogos { get; } = new();
    public EstoqueEmMemoria Estoques { get; } = new();
    public ClienteEmMemoria Clientes { get; } = new();
    public VeiculoEmMemoria Veiculos { get; } = new();
    public ServicoEmMemoria Servicos { get; } = new();

    internal static void DefinirId<T>(T entidade, int id) where T : class
        => typeof(T).GetProperty("Id")!.SetValue(entidade, id);

    internal sealed class CatalogoEmMemoria : ICatalogoRepository
    {
        private int _proximoId = 1;
        public List<Catalogo> Itens { get; } = new();
        public bool FalharAoObterTodos { get; set; }

        public Task<IEnumerable<Catalogo>> ObterTodosAsync()
        {
            if (FalharAoObterTodos)
                throw new InvalidOperationException("Banco indisponível.");

            return Task.FromResult<IEnumerable<Catalogo>>(Itens.ToList());
        }

        public Task<Catalogo?> ObterPorIdAsync(int id) => Task.FromResult(Itens.FirstOrDefault(c => c.Id == id));

        public Task AdicionarAsync(Catalogo catalogo)
        {
            DefinirId(catalogo, _proximoId++);
            Itens.Add(catalogo);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Catalogo catalogo) => Task.CompletedTask;

        public Task RemoverAsync(int id)
        {
            Itens.RemoveAll(c => c.Id == id);
            return Task.CompletedTask;
        }
    }

    internal sealed class EstoqueEmMemoria : IEstoqueRepository
    {
        private int _proximoId = 1;
        public List<Estoque> Itens { get; } = new();

        public Task<IEnumerable<Estoque>> ObterTodosAsync() => Task.FromResult<IEnumerable<Estoque>>(Itens.ToList());

        public Task<Estoque?> ObterPorIdAsync(int id) => Task.FromResult(Itens.FirstOrDefault(e => e.Id == id));

        public Task<Estoque?> ObterPorCatalogoIdAsync(int catalogoId)
            => Task.FromResult(Itens.FirstOrDefault(e => e.CatalogoId == catalogoId));

        public Task AdicionarAsync(Estoque estoque)
        {
            estoque.Id = _proximoId++;
            Itens.Add(estoque);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Estoque estoque) => Task.CompletedTask;

        public Task RemoverAsync(int id)
        {
            Itens.RemoveAll(e => e.Id == id);
            return Task.CompletedTask;
        }
    }

    internal sealed class ClienteEmMemoria : IClienteRepository
    {
        private int _proximoId = 1;
        public List<Cliente> Itens { get; } = new();
        public bool FalharAoVerificarDocumento { get; set; }

        public Task<IEnumerable<Cliente>> ObterTodosAsync() => Task.FromResult<IEnumerable<Cliente>>(Itens.ToList());

        public Task<Cliente?> ObterPorIdAsync(int id) => Task.FromResult(Itens.FirstOrDefault(c => c.Id == id));

        public Task<Cliente?> ObterPorDocumentoAsync(string numero)
            => Task.FromResult(Itens.FirstOrDefault(c => c.Documento.Numero == numero));

        public Task<bool> DocumentoExisteAsync(string numero, int? ignorarId = null)
        {
            if (FalharAoVerificarDocumento)
                throw new InvalidOperationException("Banco indisponível.");

            return Task.FromResult(Itens.Any(c => c.Documento.Numero == numero && (ignorarId == null || c.Id != ignorarId)));
        }

        public Task AdicionarAsync(Cliente cliente)
        {
            cliente.Id = _proximoId++;
            Itens.Add(cliente);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Cliente cliente) => Task.CompletedTask;

        public Task RemoverAsync(int id)
        {
            Itens.RemoveAll(c => c.Id == id);
            return Task.CompletedTask;
        }
    }

    internal sealed class VeiculoEmMemoria : IVeiculoRepository
    {
        private int _proximoId = 1;
        public List<Veiculo> Itens { get; } = new();

        public Task<IEnumerable<Veiculo>> ObterTodosAsync() => Task.FromResult<IEnumerable<Veiculo>>(Itens.ToList());

        public Task<Veiculo?> ObterPorIdAsync(int id) => Task.FromResult(Itens.FirstOrDefault(v => v.Id == id));

        public Task<Veiculo?> ObterPorPlacaAsync(string placa)
            => Task.FromResult(Itens.FirstOrDefault(v => v.Placa.Valor == placa));

        public Task<bool> PlacaExisteAsync(string placa, int? ignorarId = null)
            => Task.FromResult(Itens.Any(v => v.Placa.Valor == placa && (ignorarId == null || v.Id != ignorarId)));

        public Task AdicionarAsync(Veiculo veiculo)
        {
            veiculo.Id = _proximoId++;
            Itens.Add(veiculo);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Veiculo veiculo) => Task.CompletedTask;

        public Task RemoverAsync(int id)
        {
            Itens.RemoveAll(v => v.Id == id);
            return Task.CompletedTask;
        }
    }

    internal sealed class ServicoEmMemoria : IServicoRepository
    {
        private int _proximoId = 1;
        public List<Servico> Itens { get; } = new();

        public Task<IEnumerable<Servico>> ObterTodosAsync() => Task.FromResult<IEnumerable<Servico>>(Itens.ToList());

        public Task<Servico?> ObterPorIdAsync(int id) => Task.FromResult(Itens.FirstOrDefault(s => s.Id == id));

        public Task AdicionarAsync(Servico servico)
        {
            servico.Id = _proximoId++;
            Itens.Add(servico);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Servico servico) => Task.CompletedTask;

        public Task RemoverAsync(int id)
        {
            Itens.RemoveAll(s => s.Id == id);
            return Task.CompletedTask;
        }
    }

    // Dependências exigidas pelo registro dos serviços, mas que o seeder não usa.
    internal sealed class OrdensVazias : IOrdemServicoRepository
    {
        public Task<IEnumerable<OrdemServico>> ObterTodosAsync() => Task.FromResult<IEnumerable<OrdemServico>>(Array.Empty<OrdemServico>());
        public Task<OrdemServico?> ObterPorIdAsync(int id) => Task.FromResult<OrdemServico?>(null);
        public Task<IEnumerable<OrdemServico>> ObterPorClienteIdAsync(int clienteId) => Task.FromResult<IEnumerable<OrdemServico>>(Array.Empty<OrdemServico>());
        public Task AdicionarAsync(OrdemServico ordemServico) => Task.CompletedTask;
        public Task AtualizarAsync(OrdemServico ordemServico) => Task.CompletedTask;
        public Task RemoverAsync(int id) => Task.CompletedTask;
    }

    internal sealed class UsuariosVazios : IUsuarioRepository
    {
        public Task<IEnumerable<Usuario>> ObterTodosAsync() => Task.FromResult<IEnumerable<Usuario>>(Array.Empty<Usuario>());
        public Task<Usuario?> ObterPorIdAsync(int id) => Task.FromResult<Usuario?>(null);
        public Task<Usuario?> ObterPorUserNameAsync(string userName) => Task.FromResult<Usuario?>(null);
        public Task<bool> UserNameExisteAsync(string userName, int? ignorarId = null) => Task.FromResult(false);
        public Task AdicionarAsync(Usuario usuario) => Task.CompletedTask;
        public Task AtualizarAsync(Usuario usuario) => Task.CompletedTask;
        public Task RemoverAsync(int id) => Task.CompletedTask;
    }

    internal sealed class HasherFalso : IPasswordHasher
    {
        public string Hash(string senha) => senha;
        public bool Verificar(string senhaHash, string senhaInformada) => senhaHash == senhaInformada;
    }

    internal sealed class TokenFalso : ITokenService
    {
        public LoginResultDto GerarToken(Usuario usuario) => new() { Token = "token", ExpiraEm = DateTime.UtcNow };
    }
}
