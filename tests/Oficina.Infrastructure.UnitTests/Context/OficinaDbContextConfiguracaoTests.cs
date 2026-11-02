using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Oficina.Domain.Entities;
using Oficina.Infrastructure.Context;

namespace Oficina.Infrastructure.UnitTests.Context;

public class OficinaDbContextConfiguracaoTests
{
    // Apenas monta o modelo do EF; não abre conexão com o banco.
    private static IModel CriarModelo()
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseNpgsql("Host=localhost;Database=modelo_teste")
            .Options;

        using var context = new OficinaDbContext(options);

        return context.GetService<IDesignTimeModel>().Model;
    }

    private static IEntityType Tipo(Type entidade)
        => CriarModelo().FindEntityType(entidade) ?? throw new InvalidOperationException($"{entidade.Name} não mapeada.");

    private static IProperty Propriedade(Type entidade, string nome)
        => Tipo(entidade).FindProperty(nome) ?? throw new InvalidOperationException($"{entidade.Name}.{nome} não mapeada.");

    private static IEntityType Documento()
        => Tipo(typeof(Cliente)).FindNavigation(nameof(Cliente.Documento))!.TargetEntityType;

    private static IIndex Indice(IEntityType tipo, string propriedade)
        => tipo.GetIndexes().Single(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { propriedade }));

    [Theory]
    [InlineData(typeof(Catalogo), nameof(Catalogo.Nome), 150)]
    [InlineData(typeof(Catalogo), nameof(Catalogo.Descricao), 500)]
    [InlineData(typeof(Cliente), nameof(Cliente.Nome), 150)]
    [InlineData(typeof(Usuario), nameof(Usuario.Nome), 150)]
    [InlineData(typeof(Usuario), nameof(Usuario.UserName), 50)]
    [InlineData(typeof(Veiculo), nameof(Veiculo.Placa), 7)]
    [InlineData(typeof(Veiculo), nameof(Veiculo.Marca), 100)]
    [InlineData(typeof(Veiculo), nameof(Veiculo.Modelo), 100)]
    [InlineData(typeof(Servico), nameof(Servico.Descricao), 200)]
    [InlineData(typeof(ItemServico), nameof(ItemServico.Descricao), 200)]
    [InlineData(typeof(ItemPecaInsumo), nameof(ItemPecaInsumo.Nome), 150)]
    public void Propriedade_DeveTerOTamanhoMaximoConfigurado(Type entidade, string propriedade, int tamanho)
    {
        Assert.Equal(tamanho, Propriedade(entidade, propriedade).GetMaxLength());
    }

    [Theory]
    [InlineData(typeof(Catalogo), nameof(Catalogo.Preco))]
    [InlineData(typeof(Servico), nameof(Servico.Valor))]
    [InlineData(typeof(ItemServico), nameof(ItemServico.Valor))]
    [InlineData(typeof(ItemPecaInsumo), nameof(ItemPecaInsumo.ValorUnitario))]
    [InlineData(typeof(ItemPecaInsumo), nameof(ItemPecaInsumo.ValorTotal))]
    [InlineData(typeof(Orcamento), nameof(Orcamento.ValorServicos))]
    [InlineData(typeof(Orcamento), nameof(Orcamento.ValorPecasInsumos))]
    [InlineData(typeof(Orcamento), nameof(Orcamento.ValorTotal))]
    public void ValorMonetario_DeveTerPrecisao18Escala2(Type entidade, string propriedade)
    {
        var prop = Propriedade(entidade, propriedade);

        Assert.Equal(18, prop.GetPrecision());
        Assert.Equal(2, prop.GetScale());
    }

    [Theory]
    [InlineData(typeof(Catalogo), nameof(Catalogo.Nome))]
    [InlineData(typeof(Catalogo), nameof(Catalogo.Preco))]
    [InlineData(typeof(Cliente), nameof(Cliente.Nome))]
    [InlineData(typeof(Usuario), nameof(Usuario.Nome))]
    [InlineData(typeof(Usuario), nameof(Usuario.UserName))]
    [InlineData(typeof(Usuario), nameof(Usuario.SenhaHash))]
    [InlineData(typeof(Usuario), nameof(Usuario.Perfil))]
    [InlineData(typeof(Veiculo), nameof(Veiculo.Placa))]
    [InlineData(typeof(Veiculo), nameof(Veiculo.Marca))]
    [InlineData(typeof(Veiculo), nameof(Veiculo.Modelo))]
    [InlineData(typeof(Servico), nameof(Servico.Descricao))]
    [InlineData(typeof(OrdemServico), nameof(OrdemServico.Status))]
    [InlineData(typeof(OrdemServico), nameof(OrdemServico.DataCriacao))]
    [InlineData(typeof(Orcamento), nameof(Orcamento.Status))]
    [InlineData(typeof(Orcamento), nameof(Orcamento.DataGeracao))]
    public void Propriedade_DeveSerObrigatoria(Type entidade, string propriedade)
    {
        Assert.False(Propriedade(entidade, propriedade).IsNullable);
    }

    [Theory]
    [InlineData(typeof(Catalogo), nameof(Catalogo.Descricao))]
    [InlineData(typeof(OrdemServico), nameof(OrdemServico.DataInicioDiagnostico))]
    [InlineData(typeof(OrdemServico), nameof(OrdemServico.DataConclusaoDiagnostico))]
    [InlineData(typeof(OrdemServico), nameof(OrdemServico.DataInicioExecucao))]
    [InlineData(typeof(OrdemServico), nameof(OrdemServico.DataFinalizacao))]
    [InlineData(typeof(OrdemServico), nameof(OrdemServico.DataEntrega))]
    [InlineData(typeof(Orcamento), nameof(Orcamento.DataDisponibilizacao))]
    [InlineData(typeof(Orcamento), nameof(Orcamento.DataDecisao))]
    public void Propriedade_DevePermitirNulo(Type entidade, string propriedade)
    {
        Assert.True(Propriedade(entidade, propriedade).IsNullable);
    }

    [Fact]
    public void Cliente_DeveGravarODocumentoNasColunasDaPropriaTabela()
    {
        var documento = Documento();

        Assert.Equal("Clientes", documento.GetTableName());
        Assert.Equal("DocumentoNumero", documento.FindProperty(nameof(Oficina.Domain.ValueObjects.Documento.Numero))!.GetColumnName());
        Assert.Equal("DocumentoTipo", documento.FindProperty(nameof(Oficina.Domain.ValueObjects.Documento.Tipo))!.GetColumnName());
        Assert.Equal(14, documento.FindProperty(nameof(Oficina.Domain.ValueObjects.Documento.Numero))!.GetMaxLength());
    }

    [Fact]
    public void Cliente_DeveExigirODocumento()
    {
        var navegacao = Tipo(typeof(Cliente)).FindNavigation(nameof(Cliente.Documento))!;

        Assert.True(navegacao.ForeignKey.IsRequiredDependent);
        Assert.True(navegacao.ForeignKey.IsOwnership);
    }

    [Fact]
    public void Veiculo_DeveConverterAPlacaParaTexto()
    {
        var conversor = Propriedade(typeof(Veiculo), nameof(Veiculo.Placa)).GetValueConverter();

        Assert.NotNull(conversor);
        Assert.Equal(typeof(string), conversor!.ProviderClrType);
        Assert.Equal("ABC1D23", conversor.ConvertToProvider(new Oficina.Domain.ValueObjects.Placa("abc-1d23")));
        Assert.Equal("ABC1D23", ((Oficina.Domain.ValueObjects.Placa)conversor.ConvertFromProvider("ABC1D23")!).Valor);
    }

    [Fact]
    public void UserName_DeveSerUnico()
    {
        Assert.True(Indice(Tipo(typeof(Usuario)), nameof(Usuario.UserName)).IsUnique);
    }

    [Fact]
    public void Placa_DeveSerUnica()
    {
        Assert.True(Indice(Tipo(typeof(Veiculo)), nameof(Veiculo.Placa)).IsUnique);
    }

    [Fact]
    public void NumeroDoDocumento_DeveSerUnico()
    {
        Assert.True(Indice(Documento(), nameof(Oficina.Domain.ValueObjects.Documento.Numero)).IsUnique);
    }

    [Fact]
    public void Estoque_DeveTerUmRegistroPorItemDeCatalogo()
    {
        Assert.True(Indice(Tipo(typeof(Estoque)), nameof(Estoque.CatalogoId)).IsUnique);
    }

    [Fact]
    public void Estoque_DeveUsarXminComoTokenDeConcorrencia()
    {
        var xmin = Propriedade(typeof(Estoque), "xmin");

        Assert.True(xmin.IsConcurrencyToken);
        Assert.True(xmin.IsShadowProperty());
        Assert.Equal(ValueGenerated.OnAddOrUpdate, xmin.ValueGenerated);
    }

    [Theory]
    [InlineData(typeof(Estoque), typeof(Catalogo), DeleteBehavior.Restrict)]
    [InlineData(typeof(Veiculo), typeof(Cliente), DeleteBehavior.Restrict)]
    [InlineData(typeof(OrdemServico), typeof(Cliente), DeleteBehavior.Restrict)]
    [InlineData(typeof(OrdemServico), typeof(Veiculo), DeleteBehavior.Restrict)]
    [InlineData(typeof(ItemServico), typeof(Servico), DeleteBehavior.Restrict)]
    [InlineData(typeof(ItemPecaInsumo), typeof(Catalogo), DeleteBehavior.Restrict)]
    [InlineData(typeof(ItemServico), typeof(OrdemServico), DeleteBehavior.Cascade)]
    [InlineData(typeof(ItemPecaInsumo), typeof(OrdemServico), DeleteBehavior.Cascade)]
    [InlineData(typeof(Orcamento), typeof(OrdemServico), DeleteBehavior.Cascade)]
    public void ChaveEstrangeira_DeveTerOComportamentoDeExclusaoConfigurado(
        Type dependente, Type principal, DeleteBehavior esperado)
    {
        var chave = Tipo(dependente).GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == principal);

        Assert.Equal(esperado, chave.DeleteBehavior);
    }

    [Fact]
    public void OrdemServico_DevePossuirNoMaximoUmOrcamento()
    {
        var chave = Tipo(typeof(Orcamento)).GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(OrdemServico));

        Assert.True(chave.IsUnique);
        Assert.Equal(nameof(Orcamento.OrdemServicoId), chave.Properties.Single().Name);
    }

    [Theory]
    [InlineData(nameof(OrdemServico.ItensServico))]
    [InlineData(nameof(OrdemServico.ItensPecaInsumo))]
    public void OrdemServico_DeveAcessarAsColecoesPeloCampo(string navegacao)
    {
        var nav = Tipo(typeof(OrdemServico)).FindNavigation(navegacao)!;

        Assert.Equal(PropertyAccessMode.Field, nav.GetPropertyAccessMode());
        Assert.True(nav.IsCollection);
    }

    [Fact]
    public void Modelo_DeveConterApenasAsEntidadesEsperadas()
    {
        var tipos = CriarModelo().GetEntityTypes()
            .Where(t => !t.IsOwned())
            .Select(t => t.ClrType)
            .ToHashSet();

        Assert.Equal(
            new HashSet<Type>
            {
                typeof(Catalogo), typeof(Cliente), typeof(Estoque), typeof(OrdemServico), typeof(Servico),
                typeof(Veiculo), typeof(Usuario), typeof(ItemServico), typeof(ItemPecaInsumo), typeof(Orcamento)
            },
            tipos);
    }

    [Fact]
    public void Context_DeveExporUmDbSetParaCadaAggregateRoot()
    {
        var propriedades = typeof(OficinaDbContext).GetProperties()
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .ToHashSet();

        Assert.Equal(
            new HashSet<Type>
            {
                typeof(Catalogo), typeof(Cliente), typeof(Estoque), typeof(OrdemServico),
                typeof(Servico), typeof(Veiculo), typeof(Usuario)
            },
            propriedades);
    }
}
