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

## RN012 - Documento único do cliente

Não poderá existir mais de um cliente com o mesmo CPF ou CNPJ.

## RN013 - Placa única do veículo

Não poderá existir mais de um veículo com a mesma placa.

O veículo deverá estar associado a um cliente existente.

## RN014 - Veículo pertencente ao cliente

Na criação da Ordem de Serviço, o veículo informado deverá pertencer
ao cliente informado.

O cliente é localizado pelo documento e o veículo pela placa.

## RN015 - Itens inativos

Serviços e itens de catálogo inativos não poderão ser adicionados a uma
Ordem de Serviço.

## RN016 - Preço praticado

Ao adicionar um serviço, peça ou insumo a uma Ordem de Serviço, o
nome ou descrição e o preço vigentes deverão ser registrados no item
da Ordem de Serviço (DD001, DD010).

## RN017 - Inclusão de serviços

Serviços somente poderão ser adicionados à Ordem de Serviço nos estados
Recebida ou Em diagnóstico e antes da geração do orçamento.

## RN018 - Inclusão de peças e insumos

Peças e insumos somente poderão ser adicionados à Ordem de Serviço no
estado Em diagnóstico e antes da geração do orçamento.

## RN019 - Verificação de saldo na inclusão

Ao adicionar uma peça ou insumo à Ordem de Serviço, o saldo do estoque
deverá comportar a quantidade solicitada somada à quantidade do mesmo
item já incluída na Ordem de Serviço e ainda não consumida.

O item deverá possuir estoque cadastrado; sem estoque, a inclusão é
recusada.

Essa verificação não reserva estoque e não altera a quantidade
existente (RN007, DD005).

## RN020 - Conclusão do diagnóstico

O diagnóstico somente poderá ser concluído com a Ordem de Serviço em
diagnóstico e uma única vez.

A conclusão não altera o estado da Ordem de Serviço.

## RN021 - Geração do orçamento

O orçamento somente poderá ser gerado quando:

- a Ordem de Serviço estiver em diagnóstico;
- o diagnóstico tiver sido concluído;
- a Ordem de Serviço possuir ao menos um serviço;
- ainda não existir orçamento para a Ordem de Serviço.

O orçamento é gerado com status Pendente.

## RN022 - Disponibilização do orçamento

O orçamento somente poderá ser disponibilizado ao cliente depois de
gerado e uma única vez.

Com a disponibilização, a Ordem de Serviço passa para o estado
Aguardando aprovação.

## RN023 - Decisão única sobre o orçamento

O cliente somente poderá aprovar ou rejeitar um orçamento pendente, com
a Ordem de Serviço em Aguardando aprovação.

A decisão altera o status do orçamento, e não o estado da Ordem de
Serviço.

## RN024 - Identificação da Ordem de Serviço na decisão

A decisão do cliente sobre o orçamento é feita pelo seu CPF ou CNPJ.

Se o cliente possuir mais de uma Ordem de Serviço aguardando decisão,
deverá também informar a placa do veículo.

## RN025 - Registro de consumo

O consumo de peças e insumos somente poderá ser registrado com a Ordem
de Serviço em execução.

Cada item somente poderá ser consumido uma vez e deverá possuir estoque
cadastrado com saldo suficiente (RN008).

O registro é tudo ou nada: se algum item for inválido, já consumido ou
sem saldo, nenhum consumo é efetivado.

## RN026 - Finalização e entrega

A Ordem de Serviço somente poderá ser finalizada quando estiver em
execução e somente poderá ser entregue depois de finalizada.

## RN027 - Estoque por item de catálogo

Cada item de catálogo poderá ter um único registro de estoque.

O registro de estoque somente poderá ser criado para um item de
catálogo existente.

## RN028 - Movimentações de estoque

A quantidade de uma entrada e a quantidade de um consumo ou baixa
deverão ser maiores que zero.

## RN029 - Dados do catálogo

O item de catálogo deverá possuir nome e seu preço não poderá ser
negativo.

## RN030 - Acesso ao sistema

O acesso às operações da API exige autenticação, com exceção do login,
da consulta das Ordens de Serviço por documento e da aprovação ou
rejeição do orçamento pelo cliente.

Os perfis definem as operações permitidas:

- **Administrador**: acesso a todas as operações. São exclusivas do
  Administrador: criar a Ordem de Serviço, adicionar serviços e
  peças/insumos, iniciar a execução, entregar o veículo, listar todas
  as Ordens de Serviço, consultar os indicadores, registrar entrada de
  estoque, criar e remover estoque e administrar clientes, veículos,
  serviços, catálogo e usuários;
- **Mecânico**: iniciar e concluir o diagnóstico, gerar e disponibilizar
  o orçamento, registrar o consumo de peças e insumos, finalizar a
  Ordem de Serviço, atualizar a quantidade em estoque e registrar baixa.

A consulta de clientes, veículos, serviços, catálogo, estoque e
usuários exige apenas autenticação, sem restrição de perfil.

## RN031 - Usuários

O UserName do usuário deverá ser único.

A senha deverá conter ao menos um caractere especial e é armazenada
somente na forma de hash.

## RN032 - Tempo médio de execução

O tempo médio de execução considera somente as Ordens de Serviço
Finalizadas ou Entregues que tenham início de execução e finalização
registrados, em minutos (DD013).