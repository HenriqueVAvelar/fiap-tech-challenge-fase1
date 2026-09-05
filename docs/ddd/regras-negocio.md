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