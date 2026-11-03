# Documentação DDD

Esta pasta reúne os artefatos produzidos durante a modelagem
Domain-Driven Design do Sistema Integrado de Atendimento e
Execução de Serviços da oficina mecânica.

## Documentação Visual

A documentação visual do processo de descoberta do domínio está
disponível no Miro:

- [Board DDD - Tech Challenge Fase 1](LINK_PUBLICO_DO_MIRO)

Uma versão exportada dos principais artefatos também é mantida neste
repositório para preservar a documentação correspondente à versão
do código entregue.

## Artefatos

### Event Storming

- [Descrição do Event Storming](event-storming.md)
- [Diagramas de Event Storming](event-storming.pdf)

Os fluxos documentados contemplam:

- criação e acompanhamento da Ordem de Serviço;
- gestão de peças, insumos e estoque.

O documento descreve também os atores, a linha do tempo de comandos e
eventos de cada fluxo, as políticas, os comandos recusados e os pontos
de atenção.

### Linguagem Ubíqua

- [Linguagem Ubíqua](linguagem-ubiqua.md)

Define os principais termos utilizados no domínio e seus significados.
Inclui uma tabela de correspondência entre os termos do domínio e os
nomes utilizados no código.

### Regras de Negócio

- [Regras de Negócio](regras-negocio.md)

Registra as regras identificadas durante a descoberta e o refinamento
do domínio, incluindo as regras aplicadas na implementação (cadastro,
Ordem de Serviço, orçamento, estoque e acesso).

### Decisões de Domínio

- [Decisões de Domínio](decisoes-dominio.md)

Registra decisões tomadas durante a modelagem quando os requisitos
originais não determinavam completamente o comportamento esperado.

### Context Map

- [Context Map](context-map.md)

Documenta os Bounded Contexts identificados, o tipo de subdomínio de
cada um e os relacionamentos existentes entre eles, com diagrama e
observações sobre a implementação.

### Diagramas DDD

- [Diagramas DDD](diagramas.pdf)

Contém os diagramas produzidos durante a descoberta e modelagem
do domínio, incluindo Domain Storytelling, Bounded Contexts,
Agregados e Context Map.

## Bounded Contexts

Foram identificados os seguintes Bounded Contexts:

- Cadastro;
- Catálogo;
- Ordem de Serviço;
- Estoque;
- Identidade e Acesso.

O contexto de Ordem de Serviço foi classificado como Core Domain.

Cadastro, Catálogo e Estoque foram classificados como Supporting
Subdomains.

Identidade e Acesso foi classificado como Generic Subdomain.

O cálculo do tempo médio de execução não é um Bounded Context próprio:
é uma consulta de leitura sobre a Ordem de Serviço.

### Correspondência com o código

| Contexto | Entidades | Serviços | Controllers |
|---|---|---|---|
| Cadastro | `Cliente`, `Veiculo` | `ClienteService`, `VeiculoService` | `ClientesController`, `VeiculosController` |
| Catálogo | `Catalogo`, `Servico` | `CatalogoService`, `ServicoService` | `CatalogoController`, `ServicosController` |
| Ordem de Serviço | `OrdemServico`, `ItemServico`, `ItemPecaInsumo`, `Orcamento` | `OrdemServicoService` | `OrdensServicoController` |
| Estoque | `Estoque` | `EstoqueService` | `EstoquesController` |
| Identidade e Acesso | `Usuario` | `AuthService`, `UsuarioService` | `AuthController`, `UsuariosController` |
| Indicadores (leitura) | n/a | `TempoMedioExecucaoService` | `IndicadoresController` |

## Implementação

Os conceitos identificados durante a modelagem DDD são utilizados
como referência para a implementação do monólito.

A implementação deverá manter alinhamento com os agregados,
entidades, objetos de valor, regras e limites de contexto
documentados nesta pasta.