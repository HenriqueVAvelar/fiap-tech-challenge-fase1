# Context Map

## Objetivo

Este documento descreve os Bounded Contexts identificados durante
a descoberta do domínio e os relacionamentos existentes entre eles.

## Bounded Contexts

### Cadastro

Responsável pelo gerenciamento de clientes e veículos.

Principais conceitos:

- Cliente;
- Veículo;
- Documento;
- Placa.

### Catálogo

Responsável pelo cadastro das opções comerciais utilizadas pela oficina.

Principais conceitos:

- Serviço;
- Peça;
- Insumo;
- preço.

### Ordem de Serviço

Core Domain da aplicação.

Responsável pelo ciclo de atendimento do veículo, incluindo
diagnóstico, composição dos serviços e peças, orçamento, aprovação,
execução, finalização e entrega.

Principais conceitos:

- Ordem de Serviço;
- Item de Serviço;
- Item de Peça/Insumo;
- Orçamento;
- Status da Ordem de Serviço.

### Estoque

Responsável pelo controle quantitativo das peças e insumos.

Principais conceitos:

- quantidade física;
- quantidade reservada;
- quantidade disponível;
- entrada;
- reserva;
- consumo.

## Relacionamentos

### Cadastro → Ordem de Serviço

O contexto de Ordem de Serviço utiliza as identidades de Cliente
e Veículo provenientes do contexto de Cadastro.

### Catálogo → Ordem de Serviço

O contexto de Ordem de Serviço consulta serviços, peças e insumos
disponíveis no Catálogo e registra na OS os dados comerciais
aplicáveis no momento de sua inclusão.

### Ordem de Serviço → Estoque

Após a aprovação do orçamento, o contexto de Ordem de Serviço
solicita a reserva das peças e insumos necessários.

Durante a execução, o consumo dos itens deverá ser refletido pelo
contexto de Estoque.