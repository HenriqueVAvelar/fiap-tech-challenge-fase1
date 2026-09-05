# Descoberta do Domínio

## Contexto

A oficina mecânica atualmente realiza o controle dos atendimentos,
diagnósticos, execução de serviços e entrega dos veículos utilizando
anotações manuais e planilhas.

Esse processo gera dificuldades relacionadas ao controle das ordens
de serviço, peças, histórico de clientes e veículos, acompanhamento
dos serviços e fluxo de aprovação dos orçamentos.

## Objetivo do sistema

Desenvolver um sistema integrado para gerenciamento da oficina,
permitindo controlar clientes, veículos, ordens de serviço, serviços,
peças, estoque e orçamentos.

O sistema também deverá permitir que o cliente acompanhe o andamento
da ordem de serviço e aprove os reparos necessários.

## Escopo inicial

O MVP será composto por um back-end monolítico disponibilizando APIs REST.

## Requisitos Funcionais

| ID | Requisito |
| --- | --- |
| RF001 | O sistema deve permitir cadastrar clientes. |
| RF002 | O cliente deve poder ser identificado por CPF ou CNPJ. |
| RF003 | O sistema deve permitir cadastrar veículos. |
| RF004 | Um veículo deve possuir placa, marca, modelo e ano. |
| RF005 | O sistema deve permitir cadastrar serviços oferecidos pela oficina. |
| RF006 | O sistema deve permitir cadastrar peças e insumos. |
| RF007 | O sistema deve controlar o estoque de peças e insumos. |
| RF008 | O sistema deve permitir criar uma Ordem de Serviço. |
| RF009 | A Ordem de Serviço deve estar associada a um cliente e veículo. |
| RF010 | Deve ser possível incluir serviços na Ordem de Serviço. |
| RF011 | Deve ser possível incluir peças e insumos na Ordem de Serviço. |
| RF012 | O sistema deve calcular automaticamente o orçamento. |
| RF013 | O orçamento deve ser enviado ao cliente para aprovação. |
| RF014 | O cliente deve poder acompanhar o andamento da Ordem de Serviço. |
| RF015 | O sistema deve controlar o status da Ordem de Serviço. |
| RF016 | O sistema deve permitir consultar e detalhar Ordens de Serviço. |
| RF017 | O sistema deve permitir consultar o tempo médio de execução dos serviços. |

## Requisitos Não Funcionais

| ID | Requisito |
| --- | --- |
| RNF001 | O back-end deverá ser implementado como aplicação monolítica. |
| RNF002 | A API deverá seguir princípios REST. |
| RNF003 | A API deverá possuir documentação Swagger ou equivalente. |
| RNF004 | APIs administrativas deverão utilizar autenticação JWT. |
| RNF005 | CPF/CNPJ deverão ser validados. |
| RNF006 | A placa do veículo deverá ser validada. |
| RNF007 | O sistema deverá possuir testes unitários. |
| RNF008 | O sistema deverá possuir testes de integração. |
| RNF009 | Domínios críticos deverão possuir cobertura mínima de 80%. |
| RNF010 | A aplicação deverá possuir Dockerfile. |
| RNF011 | O ambiente deverá possuir docker-compose. |
| RNF012 | O projeto deverá possuir instruções para execução local. |