# Oficina Mecânica — Tech Challenge Fase 1

Back-end de um Sistema Integrado de Atendimento e Execução de Serviços para oficinas mecânicas, desenvolvido para o Tech Challenge da Fase 1 da pós-graduação **SOAT — Software Architecture**, da FIAP.

O projeto utiliza **C# e ASP.NET Core**, com arquitetura monolítica em camadas, modelagem orientada ao domínio com **Domain-Driven Design (DDD)** e persistência em **PostgreSQL**.

## Objetivo

Centralizar o atendimento da oficina, desde o cadastro do cliente e do veículo até a entrega do serviço, proporcionando:

- Organização das Ordens de Serviço (OS).
- Controle de serviços, peças, insumos e estoque.
- Geração de orçamentos e registro de aprovação ou rejeição.
- Acompanhamento do andamento da OS por API.
- Registro das etapas do atendimento.
- Consulta do tempo médio de execução.
- Autenticação e autorização das operações administrativas.

O escopo desta fase é o MVP do back-end. A interação com o sistema ocorre pelas APIs REST e pelo Swagger.

## Funcionalidades

### Cadastros

- Cadastro, consulta, atualização e exclusão de clientes.
- Validação de CPF/CNPJ.
- Cadastro de veículos com cliente, placa, marca, modelo e ano.
- Validação da placa do veículo.
- Cadastro de serviços e respectivos valores.
- Cadastro de peças e insumos no catálogo.
- Gerenciamento de usuários e perfis de acesso.

### Ordens de Serviço

- Criação de OS vinculada a um cliente e a um veículo.
- Validação de que o veículo pertence ao cliente informado.
- Inclusão de serviços, peças e insumos.
- Início e conclusão do diagnóstico.
- Geração do orçamento com cálculo dos valores dos itens.
- Disponibilização do orçamento para consulta.
- Registro da aprovação ou rejeição do orçamento.
- Início da execução condicionado à aprovação.
- Registro do consumo de peças e insumos.
- Finalização e entrega.
- Listagem e consulta detalhada das ordens.

Na implementação atual, a aprovação ou rejeição do orçamento é registrada por um usuário com perfil de administrador. A disponibilização ocorre pela API, sem integração de envio por e-mail ou aplicativo.

### Estoque

- Cadastro e consulta do estoque dos itens de catálogo.
- Operações de entrada e consumo.
- Validação de saldo ao incluir peças e insumos na OS.
- Baixa de estoque ao registrar o consumo durante a execução.
- Controle para impedir o registro duplicado de consumo de um mesmo item da OS.

A inclusão de uma peça no orçamento não representa uma reserva de estoque. O saldo é novamente validado no consumo.

### Indicadores

O endpoint de indicadores calcula o tempo médio, em minutos, entre o início da execução e a finalização das ordens concluídas.

São consideradas as OS com status `Finalizada` ou `Entregue` e com ambas as datas preenchidas.

## Ciclo da Ordem de Serviço

| Etapa | Ação responsável |
| --- | --- |
| Recebida | Criação da OS |
| Em diagnóstico | Início do diagnóstico |
| Aguardando aprovação | Disponibilização do orçamento |
| Em execução | Início da execução após aprovação |
| Finalizada | Finalização do serviço |
| Entregue | Registro da entrega |

As transições são realizadas por operações específicas da API e validadas pelo domínio.

A conclusão do diagnóstico e a geração do orçamento não alteram, isoladamente, o status da OS. A mudança para `AguardandoAprovacao` ocorre ao disponibilizar o orçamento.

A aprovação também não inicia a execução automaticamente: é necessário executar a operação de início da execução.

## Arquitetura

A solução é um monólito organizado em quatro projetos principais:

| Camada | Responsabilidade |
| --- | --- |
| `Oficina.Api` | Controllers, interface HTTP, autenticação, autorização, Swagger e composição das dependências |
| `Oficina.Application` | Casos de uso, serviços de aplicação, DTOs, interfaces e mapeamentos |
| `Oficina.Domain` | Entidades, objetos de valor, regras de negócio, estados e contratos de repositório |
| `Oficina.Infrastructure` | Entity Framework Core, PostgreSQL, repositórios, migrations, geração de tokens e dados iniciais |

A aplicação é publicada como uma única API e utiliza um único banco de dados.

### Modelagem DDD

A documentação identifica os seguintes Bounded Contexts:

| Contexto | Responsabilidade |
| --- | --- |
| Cadastro | Clientes e veículos |
| Catálogo | Serviços, peças e insumos |
| Ordem de Serviço | Ciclo de atendimento e execução — Core Domain |
| Estoque | Quantidades disponíveis, entradas e consumo |

Esses contextos orientam a modelagem. Na estrutura atual do código, a separação física principal é feita por camadas.

Entre os conceitos implementados estão `Cliente`, `Veiculo`, `Servico`, `Catalogo`, `Estoque`, `OrdemServico`, `Orcamento`, `ItemServico` e `ItemPecaInsumo`, além dos objetos de valor `Documento` e `Placa`.

Documentação:

- [Visão geral da arquitetura](docs/architecture/arquitetura.md)
- [ADR — Monólito modular em camadas](docs/architecture/adr/ADR-001-monolito-modular-em-camadas.md)
- [C4 — Contexto](docs/architecture/c4/c4-contexto.md)
- [C4 — Containers](docs/architecture/c4/c4-containers.md)
- [C4 — Componentes](docs/architecture/c4/c4-componentes.md)

## Tecnologias

| Tecnologia | Utilização |
| --- | --- |
| C# / .NET 10 | Desenvolvimento da aplicação |
| ASP.NET Core | API REST |
| Entity Framework Core 10 | Persistência e migrations |
| Npgsql | Integração com PostgreSQL |
| PostgreSQL 16 | Banco de dados relacional |
| JWT Bearer | Autenticação |
| ASP.NET Core Identity PasswordHasher | Hash e verificação de senhas |
| AutoMapper | Mapeamento de objetos |
| Swagger / Swashbuckle | Documentação e exploração da API |
| xUnit | Testes automatizados |
| Coverlet | Coleta de cobertura |
| Docker e Docker Compose | Ambiente em containers |
| GitHub Actions | Integração contínua |

## Banco de dados

O banco escolhido é o **PostgreSQL**.

A escolha considera:

- A natureza relacional dos dados, como os vínculos entre clientes, veículos, ordens, itens e orçamentos.
- Suporte a transações e integridade referencial.
- Integração com Entity Framework Core por meio do Npgsql.
- Disponibilidade gratuita e ampla adoção.
- Facilidade para execução local com Docker.

O esquema do banco é versionado pelas migrations em `src/Oficina.Infrastructure/Migrations`.

## Estrutura do repositório

| Caminho | Conteúdo |
| --- | --- |
| `src/Oficina.Api/` | API e configurações de execução |
| `src/Oficina.Application/` | Casos de uso e DTOs |
| `src/Oficina.Domain/` | Modelo e regras de domínio |
| `src/Oficina.Infrastructure/` | Persistência e infraestrutura |
| `tests/Oficina.Domain.UnitTests/` | Testes do domínio |
| `tests/Oficina.Application.UnitTests/` | Testes da aplicação |
| `tests/Oficina.Infrastructure.UnitTests/` | Testes de infraestrutura |
| `docs/ddd/` | Documentação DDD e diagramas exportados |
| `docs/architecture/` | Arquitetura, ADR e diagramas C4 |
| `docs/requirements/` | Requisitos |
| `docs/security/` | Diretório reservado para evidências de segurança |
| `.github/workflows/ci.yml` | Pipeline de build e testes |
| `Oficina.sln` | Solução .NET |
| `global.json` | Configuração do SDK |
| `Dockerfile` | Build da imagem da API |
| `compose.yaml` | Serviços da API e do PostgreSQL |
| `.env.example` | Modelo de variáveis do Docker Compose |

## Pré-requisitos

Para executar a API localmente com o banco em Docker:

- Git.
- .NET SDK compatível com o `global.json`: versão base `10.0.400`, com `rollForward` configurado como `latestFeature`.
- Docker com Docker Compose.
- Ferramenta `dotnet-ef` compatível com Entity Framework Core 10.
- Portas `5432` e `5027` disponíveis.

Para executar a API em container, também é utilizada a porta `8080`.

Verifique as instalações:

```bash
dotnet --version
docker --version
docker compose version
```

## Execução local

Execute os comandos a partir da raiz do repositório.

### 1. Clonar o projeto

```bash
git clone https://github.com/HenriqueVAvelar/fiap-tech-challenge-fase1.git
cd fiap-tech-challenge-fase1
```

### 2. Configurar o PostgreSQL

Copie o arquivo `.env.example` para `.env`.

No PowerShell:

```powershell
Copy-Item .env.example .env
```

No Bash:

```bash
cp .env.example .env
```

Para acompanhar este guia, configure o `.env` com:

```dotenv
POSTGRES_PASSWORD=postgres
CONNECTION_STRING=Host=db;Port=5432;Database=oficina;Username=postgres;Password=postgres
```

Esses valores são exclusivos para desenvolvimento local.

Inicie somente o banco:

```bash
docker compose up -d db
docker compose ps
```

Aguarde o serviço ficar saudável antes de aplicar as migrations.

A API executada fora do Docker utiliza `localhost` para acessar o banco. Dentro do Compose, o nome do servidor é `db`.

> Se o volume do PostgreSQL já tiver sido inicializado com outra senha, alterar o `.env` não troca a senha existente. Utilize as credenciais correspondentes ao volume.

### 3. Restaurar e compilar

```bash
dotnet restore Oficina.sln
dotnet build Oficina.sln
```

### 4. Preparar o banco

Instale a ferramenta do Entity Framework, caso ainda não esteja disponível:

```bash
dotnet tool install --global dotnet-ef --version 10.0.12
```

Se já estiver instalada, confira sua versão com `dotnet ef --version`.

Configure o ambiente como `Development`.

No PowerShell:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DOTNET_ENVIRONMENT = "Development"
```

No Bash:

```bash
export ASPNETCORE_ENVIRONMENT=Development
export DOTNET_ENVIRONMENT=Development
```

A configuração de desenvolvimento utiliza:

```text
Host=localhost;Port=5432;Database=oficina;Username=postgres;Password=postgres
```

Aplique as migrations:

```bash
dotnet ef database update --project src/Oficina.Infrastructure --startup-project src/Oficina.Api
```

As migrations devem ser aplicadas antes de iniciar a API. A inicialização atual da aplicação executa os seeds, mas não aplica migrations automaticamente.

### 5. Iniciar a API

```bash
dotnet run --project src/Oficina.Api --launch-profile http
```

Acesse:

- API: http://localhost:5027
- Swagger: http://localhost:5027/swagger
- OpenAPI: http://localhost:5027/swagger/v1/swagger.json

O Swagger é habilitado somente no ambiente `Development`.

Para utilizar o perfil HTTPS:

```bash
dotnet dev-certs https --trust
dotnet run --project src/Oficina.Api --launch-profile https
```

Nesse perfil, o Swagger também fica disponível em https://localhost:7064/swagger.

## Execução da API com Docker

O repositório contém um `Dockerfile` e um `compose.yaml`.

Na configuração atual, o Compose utiliza `Production` e passa a conexão em `ConnectionStrings__default`, enquanto a aplicação lê `ConnectionStrings__DefaultConnection`. Além disso, a chave JWT precisa ter pelo menos 32 caracteres.

Para executar a demonstração local com Swagger, crie na raiz um arquivo `compose.override.yaml` com:

```yaml
services:
  web_api:
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__DefaultConnection: ${CONNECTION_STRING}
      Jwt__Secret: chave-somente-para-desenvolvimento-local-0123456789
      DemoData__Habilitado: "true"
```

Esse arquivo complementa o `compose.yaml` e é carregado automaticamente pelo Docker Compose.

Após preparar o `.env` e aplicar as migrations conforme a seção anterior, execute:

```bash
docker compose up -d --build
```

Acesse o Swagger em:

http://localhost:8080/swagger

Para acompanhar os logs:

```bash
docker compose logs -f web_api
```

Para encerrar os containers:

```bash
docker compose down
```

O volume do PostgreSQL é preservado por esse comando.

## Autenticação e autorização

A API utiliza JWT Bearer.

Na primeira inicialização com o banco preparado, o seed tenta criar um administrador, desde que ainda não exista um usuário com esse perfil.

Credenciais padrão para demonstração local:

| Campo | Valor |
| --- | --- |
| Usuário | `admin` |
| Senha | `Admin@123` |

Essas credenciais e a chave JWT de desenvolvimento não devem ser utilizadas em produção.

### Obter o token

Execute `POST /Auth/login` com:

```json
{
  "userName": "admin",
  "senha": "Admin@123"
}
```

A resposta contém os campos `token` e `expiraEm`.

### Utilizar o token no Swagger

1. Execute o login.
2. Copie o valor do campo `token`.
3. Clique em **Authorize**.
4. Cole somente o token JWT e confirme.
5. Execute os endpoints protegidos.

Em outros clientes HTTP, envie:

```http
Authorization: Bearer <token>
```

### Perfis

As permissões variam conforme a operação:

- **Administrador:** operações administrativas, gerenciamento de usuários e controle do fluxo da OS.
- **Mecânico:** operações autorizadas de diagnóstico, orçamento, consumo e finalização.
- Algumas consultas exigem apenas um usuário autenticado.
- `GET /OrdensServico/{id}` permite consulta sem autenticação na implementação atual.

## Dados de demonstração

Em `Development`, a configuração `DemoData:Habilitado` está ativada.

O seed prepara dados para exploração da API, incluindo:

- Cliente.
- Veículo.
- Serviço.
- Item de catálogo e estoque.
- Ordem de Serviço com serviço e peça incluídos.

Em uma base nova, a OS de demonstração é avançada para `EmDiagnostico` para permitir a inclusão da peça.

Consulte os IDs retornados pela API; não presuma valores fixos.

Para desabilitar os dados de demonstração, configure `DemoData:Habilitado` como `false` ou utilize a variável de ambiente `DemoData__Habilitado=false`.

## Principais endpoints

As rotas não possuem o prefixo `/api`.

| Recurso | Rota base |
| --- | --- |
| Autenticação | `/Auth/login` |
| Clientes | `/Clientes` |
| Veículos | `/Veiculos` |
| Serviços | `/Servicos` |
| Peças e insumos | `/Catalogo` |
| Estoque | `/Estoques` |
| Ordens de Serviço | `/OrdensServico` |
| Usuários | `/Usuarios` |
| Tempo médio de execução | `/Indicadores/tempo-medio-execucao` |

Consulte o Swagger para os contratos de entrada, respostas e operações disponíveis.

### Roteiro do fluxo principal

Para uma nova OS:

1. Autenticar como administrador.
2. Cadastrar ou consultar cliente e veículo.
3. Cadastrar ou consultar serviços, catálogo e estoque.
4. Criar a OS com `POST /OrdensServico`.
5. Adicionar serviços com `POST /OrdensServico/{id}/servicos`.
6. Iniciar o diagnóstico com `POST /OrdensServico/{id}/iniciar-diagnostico`.
7. Adicionar peças e insumos com `POST /OrdensServico/{id}/pecas-insumos`.
8. Concluir o diagnóstico com `POST /OrdensServico/{id}/concluir-diagnostico`.
9. Gerar o orçamento com `POST /OrdensServico/{id}/gerar-orcamento`.
10. Disponibilizar o orçamento com `POST /OrdensServico/{id}/disponibilizar-orcamento`.
11. Registrar a aprovação com `POST /OrdensServico/{id}/aprovar-orcamento`.
12. Iniciar a execução com `POST /OrdensServico/{id}/iniciar-execucao`.
13. Registrar os itens consumidos com `POST /OrdensServico/{id}/consumir-pecas-insumos`.
14. Finalizar com `POST /OrdensServico/{id}/finalizar`.
15. Registrar a entrega com `POST /OrdensServico/{id}/entregar`.

A rejeição pode ser registrada por `POST /OrdensServico/{id}/rejeitar-orcamento`, impedindo o início da execução sem aprovação.

O andamento pode ser consultado com `GET /OrdensServico/{id}`.

## Testes automatizados

Executar todos os testes:

```bash
dotnet test Oficina.sln --configuration Release
```

Executar um projeto específico:

```bash
dotnet test tests/Oficina.Domain.UnitTests/Oficina.Domain.UnitTests.csproj
```

Os projetos existentes incluem testes de regras de OS, consumo e estoque, documentos, usuários, autenticação, indicadores, mapeamentos e componentes de infraestrutura.

### Cobertura

Para coletar cobertura com o Coverlet:

```bash
dotnet test Oficina.sln --configuration Release --collect:"XPlat Code Coverage" --results-directory TestResults
```

Os arquivos `coverage.cobertura.xml` são gerados em subdiretórios de `TestResults`.

A exigência da Fase 1 é cobertura mínima de **80% nos domínios críticos**. A existência do coletor não comprova esse percentual: o resultado deve ser medido e apresentado como evidência da entrega.

Na versão documentada, não há uma suíte de integração dos fluxos principais identificada no repositório. Essa é uma pendência em relação aos requisitos da fase.

## Integração contínua

O workflow em [.github/workflows/ci.yml](.github/workflows/ci.yml) executa:

1. Restauração das dependências.
2. Build em Release.
3. Testes automatizados.

O pipeline é acionado em Pull Requests para `main`, pushes em `main` e execução manual.

A configuração atual não publica relatório de cobertura nem aplica uma barreira mínima de 80%.

## Documentação

### Domínio e requisitos

- [Documentação DDD](docs/ddd/README.md)
- [Event Storming — descrição](docs/ddd/event-storming.md)
- [Event Storming — PDF](docs/ddd/event-storming.pdf)
- [Diagramas DDD — PDF](docs/ddd/diagramas.pdf)
- [Linguagem Ubíqua](docs/ddd/linguagem-ubiqua.md)
- [Regras de negócio](docs/ddd/regras-negocio.md)
- [Decisões de domínio](docs/ddd/decisoes-dominio.md)
- [Context Map](docs/ddd/context-map.md)
- [Requisitos funcionais e não funcionais](docs/requirements/requisitos.md)

**Board público de modelagem:** pendente de inclusão. O índice de DDD ainda contém um marcador para o link do Miro.

### Segurança

O diretório [docs/security](docs/security/) está reservado para documentação e evidências de segurança.

**Relatório de análise de vulnerabilidades:** pendente de inclusão.

## Entrega da Fase 1

- **Curso:** SOAT — Software Architecture.
- **Fase:** 1.
- **Grupo:** a preencher.
- **Integrantes, RMs e usernames do Discord:** a preencher.
- **Vídeo público no YouTube:** link a incluir.
- **Branch da entrega:** a definir.
- **Hash do commit da entrega:** a definir.

Para identificar o commit selecionado:

```bash
git rev-parse HEAD
```

O PDF de entrega deve informar a branch e o hash exatos, com links para o repositório, a documentação e o vídeo correspondentes à mesma versão.

### Pendências de evidências e preparação da entrega

- Publicar o link do vídeo, com duração máxima de 15 minutos.
- Preencher a identificação do grupo e dos integrantes.
- Substituir o marcador do link público de modelagem.
- Implementar e apresentar testes de integração dos fluxos principais.
- Medir e comprovar cobertura mínima de 80% nos domínios críticos.
- Incluir o resultado da análise de vulnerabilidades.
- Consolidar a configuração do Docker para execução completa.
- Validar a execução a partir de um clone e de um banco novos.
- Confirmar que o repositório estará público durante a correção.
- Confirmar a proteção da `main` e a entrada de alterações por Pull Request.
- Registrar a versão final no PDF de entrega.

## Contexto acadêmico

Projeto desenvolvido para aplicação dos conhecimentos de arquitetura de software, DDD, desenvolvimento de APIs, persistência, segurança e qualidade de software na Fase 1 do Tech Challenge da FIAP.
