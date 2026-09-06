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

Uma Ordem de Serviço que necessita aprovação do orçamento somente poderá
iniciar sua execução depois da aprovação do cliente.

## RN006 - Estados da Ordem de Serviço

A Ordem de Serviço poderá assumir os seguintes estados:

- Recebida;
- Em diagnóstico;
- Aguardando aprovação;
- Em execução;
- Finalizada;
- Entregue.

## RN007 - Reserva de estoque

Após a aprovação do orçamento, as peças e insumos necessários
deverão ser reservados, desde que exista quantidade disponível.

## RN008 - Disponibilidade para reserva

Não será permitida a reserva de uma quantidade superior à
quantidade disponível do item.

## RN009 - Consumo de estoque

O estoque físico deverá ser reduzido quando uma peça ou insumo
reservado for efetivamente consumido pela Ordem de Serviço.

## RN010 - Quantidade de estoque

As quantidades física e reservada de um item de estoque não
poderão assumir valores negativos.