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

### Linguagem Ubíqua

- [Linguagem Ubíqua](linguagem-ubiqua.md)

Define os principais termos utilizados no domínio e seus significados.

### Regras de Negócio

- [Regras de Negócio](regras-negocio.md)

Registra as regras identificadas durante a descoberta e o refinamento
do domínio.

### Decisões de Domínio

- [Decisões de Domínio](decisoes-dominio.md)

Registra decisões tomadas durante a modelagem quando os requisitos
originais não determinavam completamente o comportamento esperado.

### Context Map

- [Context Map](context-map.md)

Documenta os Bounded Contexts identificados e os relacionamentos
existentes entre eles.

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
- Estoque.

O contexto de Ordem de Serviço foi classificado como Core Domain.

Cadastro, Catálogo e Estoque foram classificados como Supporting
Subdomains.

## Implementação

Os conceitos identificados durante a modelagem DDD são utilizados
como referência para a implementação do monólito.

A implementação deverá manter alinhamento com os agregados,
entidades, objetos de valor, regras e limites de contexto
documentados nesta pasta.