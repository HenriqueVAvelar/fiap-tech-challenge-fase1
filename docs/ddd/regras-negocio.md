# Regras de Negócio

## RN001 - Identificação do cliente

O cliente deverá ser identificado através de CPF ou CNPJ válido.

## RN002 - Veículo da Ordem de Serviço

Toda Ordem de Serviço deverá possuir um veículo associado.

## RN003 - Cliente da Ordem de Serviço

Toda Ordem de Serviço deverá estar associada ao cliente responsável
pelo veículo.

## RN004 - Composição do orçamento

O valor do orçamento deverá ser calculado automaticamente considerando
os serviços, peças e insumos incluídos na Ordem de Serviço.

## RN005 - Execução após aprovação

Uma Ordem de Serviço somente poderá iniciar sua execução depois da
aprovação do orçamento pelo cliente.

A rejeição do orçamento não autoriza o início da execução.

## RN006 - Estados da Ordem de Serviço

A Ordem de Serviço poderá assumir os seguintes estados:

- Recebida;
- Em diagnóstico;
- Aguardando aprovação;
- Em execução;
- Finalizada;
- Entregue.

## RN007 - Alteração do estoque

A inclusão de uma peça ou insumo em uma Ordem de Serviço não deverá
alterar automaticamente a quantidade em estoque.

A baixa ocorrerá somente quando o item for efetivamente consumido.

## RN008 - Disponibilidade para consumo

Não será permitido consumir uma quantidade de peça ou insumo superior
à quantidade existente em estoque.

## RN009 - Consumo de estoque

A quantidade em estoque deverá ser reduzida quando uma peça ou insumo
for efetivamente consumido durante a execução da Ordem de Serviço.

## RN010 - Quantidade de estoque

A quantidade em estoque de uma peça ou insumo não poderá assumir
valor negativo.

## RN011 - Decisão sobre o orçamento

O cliente poderá aprovar ou rejeitar o orçamento disponibilizado
para sua análise.