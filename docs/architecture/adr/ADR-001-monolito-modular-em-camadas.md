# ADR-001 - Utilização de Monólito Modular em Camadas

## Status

Aceito

## Contexto

O Tech Challenge requer a implementação de um back-end monolítico
para o MVP do Sistema Integrado de Atendimento e Execução de Serviços.

Durante a modelagem de domínio foram identificados os Bounded Contexts
de Cadastro, Catálogo, Ordem de Serviço e Estoque.

É necessário escolher uma organização arquitetural que mantenha
a simplicidade de implantação exigida pelo MVP sem perder a separação
entre as responsabilidades identificadas durante o DDD.

## Decisão

A aplicação será implementada como um monólito modular utilizando
arquitetura em camadas.

As principais camadas serão:

- Domain;
- Application;
- Infrastructure;
- API.

Os Bounded Contexts serão preservados através da organização modular
interna das camadas.

A aplicação será construída e implantada como uma única unidade.

## Consequências Positivas

- menor complexidade operacional para o MVP;
- implantação simples;
- preservação das fronteiras identificadas durante o DDD;
- separação entre regras de negócio e detalhes de infraestrutura;
- maior facilidade para criação de testes unitários no domínio;
- possibilidade de evolução futura dos módulos.

## Consequências Negativas

- os módulos compartilham o mesmo processo de aplicação;
- uma falha grave pode afetar toda a aplicação;
- o banco de dados será compartilhado;
- é necessário manter disciplina arquitetural para evitar
  acoplamento excessivo entre os módulos.

## Alternativas Consideradas

### Monólito sem separação modular

Foi descartado por facilitar o acoplamento entre regras de diferentes
áreas do domínio e dificultar a manutenção das fronteiras identificadas
durante o DDD.

### Microserviços

Foram descartados por adicionarem complexidade operacional,
distribuição de dados e comunicação entre serviços desnecessárias
para o escopo do MVP, além de não corresponderem ao requisito
de implementação monolítica do Tech Challenge.