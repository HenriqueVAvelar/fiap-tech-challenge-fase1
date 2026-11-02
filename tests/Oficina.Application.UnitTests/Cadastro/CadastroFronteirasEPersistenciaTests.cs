using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Oficina.Application.DTOs;
using Oficina.Application.DTOs.Usuario;
using Oficina.Application.Interfaces;
using Oficina.Application.Mappings;
using Oficina.Application.Services;
using Oficina.Application.UnitTests.Fakes;
using Oficina.Domain.Entities;
using Oficina.Domain.Enums;
using Oficina.Domain.Interfaces;
using Oficina.Domain.ValueObjects;
using CatalogoEntity = Oficina.Domain.Entities.Catalogo;
using UsuarioEntity = Oficina.Domain.Entities.Usuario;

namespace Oficina.Application.UnitTests.Cadastro;

// Valores no limite (zero), consulta de registro inexistente e chamada ao repositório ao atualizar.
public class CadastroFronteirasEPersistenciaTests
{
    private const string Cpf = "529.982.247-25";

    private readonly IMapper _mapper = CadastroFakes.CriarMapper();

    #region Estoque
    private (EstoqueService Service, FakeEstoqueRepository Repositorio) CriarEstoqueService()
    {
        var repositorio = new FakeEstoqueRepository();
        var catalogos = new FakeCatalogoRepository();
        var catalogo = new CatalogoEntity("Alternador", 900m, EnumsConsumivel.TipoConsumivel.Peca, true);
        catalogos.AdicionarAsync(catalogo).GetAwaiter().GetResult();

        return (new EstoqueService(repositorio, catalogos, _mapper), repositorio);
    }

    [Fact]
    public async Task Estoque_ObterPorId_DeveRetornarNull_QuandoNaoExiste()
    {
        var (service, _) = CriarEstoqueService();

        Assert.Null(await service.ObterPorIdAsync(99));
    }

    [Fact]
    public async Task Estoque_Atualizar_DeveAceitarQuantidadeZero_EPersistir()
    {
        var (service, repositorio) = CriarEstoqueService();
        var criado = await service.CriarAsync(new EstoqueInputDto { CatalogoId = 1, Quantidade = 5 });

        var ok = await service.AtualizarAsync(criado.Id, new EstoqueUpdateInputDto { Quantidade = 0 });

        Assert.True(ok);
        Assert.Equal(0, repositorio.Estoques.Single().Quantidade);
        Assert.Equal(1, repositorio.Atualizacoes);
    }

    [Fact]
    public async Task Estoque_Atualizar_DeveLancarExcecaoComMensagem_QuandoQuantidadeNegativa()
    {
        var (service, repositorio) = CriarEstoqueService();
        var criado = await service.CriarAsync(new EstoqueInputDto { CatalogoId = 1, Quantidade = 5 });

        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.AtualizarAsync(criado.Id, new EstoqueUpdateInputDto { Quantidade = -1 }));

        Assert.Contains("A quantidade não pode ser negativa.", ex.Message);
        Assert.Equal(5, repositorio.Estoques.Single().Quantidade);
        Assert.Equal(0, repositorio.Atualizacoes);
    }

    [Fact]
    public async Task Estoque_Criar_DeveLancarExcecaoComMensagem_QuandoCatalogoNaoExiste()
    {
        var (service, _) = CriarEstoqueService();

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CriarAsync(new EstoqueInputDto { CatalogoId = 77, Quantidade = 1 }));

        Assert.Equal("Item de catálogo 77 não encontrado.", ex.Message);
    }
    #endregion

    #region Serviço
    private (ServicoService Service, FakeServicoRepository Repositorio) CriarServicoService()
    {
        var repositorio = new FakeServicoRepository();
        return (new ServicoService(repositorio, _mapper), repositorio);
    }

    [Fact]
    public async Task Servico_ObterPorId_DeveRetornarNull_QuandoNaoExiste()
    {
        var (service, _) = CriarServicoService();

        Assert.Null(await service.ObterPorIdAsync(99));
    }

    [Fact]
    public async Task Servico_Criar_DeveAceitarValorZero()
    {
        var (service, repositorio) = CriarServicoService();

        var dto = await service.CriarAsync(new ServicoInputDto { Descricao = "Cortesia", Valor = 0m, Ativo = true });

        Assert.Equal(0m, dto.Valor);
        Assert.Single(repositorio.Servicos);
    }

    [Fact]
    public async Task Servico_Criar_DeveLancarExcecaoComMensagem_QuandoValorNegativo()
    {
        var (service, repositorio) = CriarServicoService();

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CriarAsync(new ServicoInputDto { Descricao = "Revisão", Valor = -0.01m }));

        Assert.Contains("O valor do serviço não pode ser negativo.", ex.Message);
        Assert.Empty(repositorio.Servicos);
    }

    [Fact]
    public async Task Servico_Criar_DeveLancarExcecaoComMensagem_QuandoDescricaoVazia()
    {
        var (service, _) = CriarServicoService();

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CriarAsync(new ServicoInputDto { Descricao = "   ", Valor = 10m }));

        Assert.Contains("A descrição do serviço é obrigatória.", ex.Message);
    }

    [Fact]
    public async Task Servico_Atualizar_DeveAceitarValorZero_EPersistir()
    {
        var (service, repositorio) = CriarServicoService();
        var criado = await service.CriarAsync(new ServicoInputDto { Descricao = "Revisão", Valor = 100m, Ativo = true });

        var ok = await service.AtualizarAsync(
            criado.Id, new ServicoInputDto { Descricao = "Revisão", Valor = 0m, Ativo = false });

        Assert.True(ok);
        Assert.Equal(0m, repositorio.Servicos.Single().Valor);
        Assert.False(repositorio.Servicos.Single().Ativo);
        Assert.Equal(1, repositorio.Atualizacoes);
    }
    #endregion

    #region Cliente
    private (ClienteService Service, FakeClienteRepository Repositorio) CriarClienteService()
    {
        var repositorio = new FakeClienteRepository();
        return (new ClienteService(repositorio, _mapper), repositorio);
    }

    [Fact]
    public async Task Cliente_ObterPorDocumento_DeveRetornarNull_QuandoNaoExiste()
    {
        var (service, _) = CriarClienteService();

        Assert.Null(await service.ObterPorDocumentoAsync(Cpf));
    }

    [Fact]
    public async Task Cliente_Atualizar_DevePersistir()
    {
        var (service, repositorio) = CriarClienteService();
        await service.CriarAsync(new ClienteInputDto { Nome = "Maria", Documento = Cpf });

        var ok = await service.AtualizarAsync(Cpf, new ClienteInputDto { Nome = "Maria Souza", Documento = Cpf });

        Assert.True(ok);
        Assert.Equal("Maria Souza", repositorio.Clientes.Single().Nome);
        Assert.Equal(1, repositorio.Atualizacoes);
    }

    [Fact]
    public async Task Cliente_Criar_DeveLancarExcecaoComMensagem_QuandoDocumentoJaExiste()
    {
        var (service, _) = CriarClienteService();
        await service.CriarAsync(new ClienteInputDto { Nome = "Maria", Documento = Cpf });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CriarAsync(new ClienteInputDto { Nome = "Outra", Documento = "52998224725" }));

        Assert.Equal($"Já existe um cliente com o documento '{new Documento(Cpf).Formatado()}'.", ex.Message);
    }
    #endregion

    #region Veículo
    private (VeiculoService Service, FakeVeiculoRepository Repositorio, int ClienteId) CriarVeiculoService()
    {
        var veiculos = new FakeVeiculoRepository();
        var clientes = new FakeClienteRepository();
        var cliente = new Cliente("Maria", new Documento(Cpf));
        clientes.AdicionarAsync(cliente).GetAwaiter().GetResult();

        return (new VeiculoService(veiculos, clientes, _mapper), veiculos, cliente.Id);
    }

    private static VeiculoInputDto Veiculo(int clienteId, string placa = "ABC1D23")
        => new() { ClienteId = clienteId, Placa = placa, Marca = "Fiat", Modelo = "Uno", Ano = 2020 };

    [Fact]
    public async Task Veiculo_ObterPorPlaca_DeveRetornarNull_QuandoNaoExiste()
    {
        var (service, _, _) = CriarVeiculoService();

        Assert.Null(await service.ObterPorPlacaAsync("ABC1D23"));
    }

    [Fact]
    public async Task Veiculo_Atualizar_DevePersistir()
    {
        var (service, repositorio, clienteId) = CriarVeiculoService();
        await service.CriarAsync(Veiculo(clienteId));

        var ok = await service.AtualizarAsync("ABC1D23", Veiculo(clienteId));

        Assert.True(ok);
        Assert.Equal(1, repositorio.Atualizacoes);
    }

    [Fact]
    public async Task Veiculo_Criar_DeveLancarExcecoesComMensagem_QuandoClienteInexistenteOuPlacaDuplicada()
    {
        var (service, _, clienteId) = CriarVeiculoService();

        var semCliente = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CriarAsync(Veiculo(999)));
        Assert.Equal("Cliente 999 não encontrado.", semCliente.Message);

        await service.CriarAsync(Veiculo(clienteId));
        var duplicada = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CriarAsync(Veiculo(clienteId)));
        Assert.Equal("Já existe um veículo com a placa 'ABC1D23'.", duplicada.Message);
    }
    #endregion

    #region Usuário
    private (UsuarioService Service, ContadorUsuarioRepository Repositorio) CriarUsuarioService()
    {
        var expression = new MapperConfigurationExpression();
        expression.AddProfile<UsuarioProfile>();
        var mapper = new MapperConfiguration(expression, NullLoggerFactory.Instance).CreateMapper();
        var repositorio = new ContadorUsuarioRepository();

        return (new UsuarioService(repositorio, new HasherFalso(), mapper), repositorio);
    }

    private static UsuarioInputDto NovoUsuario(string userName = "admin")
        => new()
        {
            Nome = "Administrador",
            UserName = userName,
            Senha = "Senha@123",
            Perfil = EnumsUsuario.PerfilUsuario.Administrador
        };

    [Fact]
    public async Task Usuario_ObterPorId_DeveRetornarNull_QuandoNaoExiste()
    {
        var (service, _) = CriarUsuarioService();

        Assert.Null(await service.ObterPorIdAsync(99));
    }

    [Fact]
    public async Task Usuario_Atualizar_DevePersistirAAlteracao()
    {
        var (service, repositorio) = CriarUsuarioService();
        var criado = await service.CriarAsync(NovoUsuario());

        var ok = await service.AtualizarAsync(criado.Id, new UsuarioUpdateInputDto
        {
            Nome = "Novo nome",
            UserName = "admin",
            Perfil = EnumsUsuario.PerfilUsuario.Mecanico
        });

        Assert.True(ok);
        Assert.Equal(1, repositorio.Atualizacoes);
    }

    [Fact]
    public async Task Usuario_Criar_DeveLancarExcecaoComMensagem_QuandoUserNameJaEstaEmUso()
    {
        var (service, _) = CriarUsuarioService();
        await service.CriarAsync(NovoUsuario("admin"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CriarAsync(NovoUsuario("  admin  ")));

        Assert.Equal("O UserName 'admin' já está em uso.", ex.Message);
    }

    private sealed class HasherFalso : IPasswordHasher
    {
        public string Hash(string senha) => $"hash:{senha}";
        public bool Verificar(string senhaHash, string senhaInformada) => senhaHash == Hash(senhaInformada);
    }

    private sealed class ContadorUsuarioRepository : IUsuarioRepository
    {
        private int _proximoId = 1;
        public List<UsuarioEntity> Usuarios { get; } = new();
        public int Atualizacoes { get; private set; }

        public Task<IEnumerable<UsuarioEntity>> ObterTodosAsync()
            => Task.FromResult<IEnumerable<UsuarioEntity>>(Usuarios.ToList());

        public Task<UsuarioEntity?> ObterPorIdAsync(int id) => Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id));

        public Task<UsuarioEntity?> ObterPorUserNameAsync(string userName)
            => Task.FromResult(Usuarios.FirstOrDefault(u =>
                string.Equals(u.UserName, userName.Trim(), StringComparison.OrdinalIgnoreCase)));

        public Task<bool> UserNameExisteAsync(string userName, int? ignorarId = null)
            => Task.FromResult(Usuarios.Any(u =>
                string.Equals(u.UserName, userName.Trim(), StringComparison.OrdinalIgnoreCase)
                && (ignorarId == null || u.Id != ignorarId)));

        public Task AdicionarAsync(UsuarioEntity usuario)
        {
            typeof(UsuarioEntity).GetProperty(nameof(UsuarioEntity.Id))!.SetValue(usuario, _proximoId++);
            Usuarios.Add(usuario);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(UsuarioEntity usuario)
        {
            Atualizacoes++;
            return Task.CompletedTask;
        }

        public Task RemoverAsync(int id)
        {
            Usuarios.RemoveAll(u => u.Id == id);
            return Task.CompletedTask;
        }
    }
    #endregion
}
