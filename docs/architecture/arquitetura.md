# Arquitetura da Solução

## Visão Geral

O Sistema Integrado de Atendimento e Execução de Serviços será
implementado como um monólito modular utilizando arquitetura em camadas.

A aplicação será desenvolvida utilizando ASP.NET Core e utilizará
PostgreSQL como banco de dados relacional.

A divisão modular da aplicação será orientada pelos Bounded Contexts
identificados durante a modelagem de domínio:

- Cadastro;
- Catálogo;
- Ordem de Serviço;
- Estoque.

O contexto de Ordem de Serviço representa o Core Domain da solução.

## Estilo Arquitetural

A solução será implementada como um monólito modular.

O sistema será construído e implantado como uma única aplicação,
mas suas responsabilidades internas serão separadas em módulos
orientados pelos contextos do domínio.

Essa estratégia foi escolhida por permitir simplicidade operacional
adequada ao MVP, ao mesmo tempo em que mantém separação de
responsabilidades e limites claros entre os diferentes conceitos
do negócio.

## Camadas

### Domain

Contém o modelo e as regras de domínio.

Principais responsabilidades:

- Aggregate Roots;
- entidades;
- Value Objects;
- regras de negócio;
- invariantes;
- conceitos de domínio.

A camada Domain não deverá depender de tecnologias de persistência,
HTTP ou frameworks de apresentação.

### Application

Contém os casos de uso da aplicação e realiza a orquestração entre
o domínio e os recursos externos necessários.

Essa camada não deverá conter regras que pertencem aos agregados
do domínio.

### Infrastructure

Contém as implementações relacionadas a detalhes técnicos externos
ao domínio.

Entre suas responsabilidades estão:

- persistência;
- Entity Framework Core;
- acesso ao PostgreSQL;
- implementação de repositórios.

### API

Representa a interface HTTP da aplicação.

Entre suas responsabilidades estão:

- exposição das APIs REST;
- autenticação e autorização;
- configuração de JWT;
- documentação Swagger/OpenAPI;
- conversão das requisições HTTP para chamadas dos casos de uso.

## Dependências

As dependências entre as camadas deverão seguir a direção das
regras de negócio.

Domain não dependerá das demais camadas.

Application dependerá de Domain.

Infrastructure poderá depender de Domain e Application para
implementar os contratos necessários.

API utilizará Application e Infrastructure para disponibilizar
os casos de uso através de HTTP.

## Persistência

A aplicação utilizará PostgreSQL como banco de dados relacional.

A escolha do PostgreSQL considera:

- suporte a transações;
- adequação aos dados relacionais do domínio;
- utilização gratuita;
- ampla adoção;
- integração com Entity Framework Core;
- facilidade de execução utilizando Docker.

Para o MVP será utilizado um único banco de dados para todo o monólito.

## Bounded Contexts

A organização interna da aplicação deverá preservar os Bounded
Contexts identificados durante a modelagem DDD.

### Cadastro

Gerenciamento de clientes e veículos.

### Catálogo

Gerenciamento dos serviços, peças e insumos disponíveis.

### Ordem de Serviço

Core Domain responsável pelo ciclo de atendimento do veículo.

### Estoque

Controle das quantidades físicas, reservadas e disponíveis dos
itens utilizados pela oficina.