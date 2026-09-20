# Linguagem Ubíqua

Este documento define os principais termos utilizados no domínio
do sistema da oficina mecânica.

A linguagem aqui definida deverá ser utilizada de forma consistente
na documentação, código-fonte e comunicação relacionada ao sistema.

## Cliente

Pessoa física ou jurídica que solicita serviços à oficina e possui
um ou mais veículos cadastrados.

O cliente é identificado por CPF ou CNPJ.

## Veículo

Veículo pertencente a um cliente e atendido pela oficina.

É identificado por informações como placa, marca, modelo e ano.

## Ordem de Serviço

Registro responsável por representar o atendimento de um veículo
pela oficina.

A Ordem de Serviço registra o veículo, cliente, serviços solicitados,
peças utilizadas, orçamento e andamento do atendimento.

## Serviço

Tipo de trabalho que pode ser realizado pela oficina.

Exemplos:

- troca de óleo;
- alinhamento;
- balanceamento;
- substituição de componentes.

Um serviço possui um valor utilizado na composição do orçamento.

## Peça

Item físico utilizado durante a execução de um serviço.

Uma peça possui informações de identificação, preço e disponibilidade
em estoque.

## Insumo

Material consumível utilizado durante a execução dos serviços.

## Estoque

Representa a disponibilidade das peças e insumos da oficina.

## Diagnóstico

Etapa da Ordem de Serviço na qual a oficina analisa o veículo e
identifica os serviços e peças necessários.

## Orçamento

Valor calculado para execução da Ordem de Serviço.

O orçamento é composto pelos serviços e pelas peças/insumos incluídos
na Ordem de Serviço.

## Aprovação do Orçamento

Decisão do cliente autorizando a execução dos serviços apresentados
no orçamento.

## Status da Ordem de Serviço

Representa a situação atual de uma Ordem de Serviço durante o fluxo
de atendimento.

Os estados definidos são:

- Recebida;
- Em diagnóstico;
- Aguardando aprovação;
- Em execução;
- Finalizada;
- Entregue.

## Quantidade

Quantidade de uma peça ou insumo existente no estoque.

É aumentada pelo registro de entrada e reduzida pelo consumo
ocorrido durante a execução de uma Ordem de Serviço.

A quantidade não poderá assumir valor negativo.

## Entrada de Estoque

Operação que registra o recebimento de peças ou insumos pela oficina,
aumentando a quantidade existente em estoque.

## Consumo de Estoque

Operação que registra a utilização de uma peça ou insumo durante a
execução de uma Ordem de Serviço, reduzindo a quantidade existente
em estoque.
