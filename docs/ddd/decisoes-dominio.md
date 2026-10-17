# Decisões de Domínio

## DD001 - Valores de serviços na Ordem de Serviço

Ao adicionar um serviço a uma Ordem de Serviço, o valor vigente
do serviço deverá ser registrado no item da Ordem de Serviço.

Alterações futuras no preço do serviço no catálogo não deverão
alterar Ordens de Serviço existentes.

## DD002 - Referência ao Cliente

A Ordem de Serviço manterá referência ao Cliente através de sua
identidade, não contendo o agregado Cliente completo.

## DD003 - Referência ao Veículo

A Ordem de Serviço manterá referência ao Veículo através de sua
identidade.

## DD004 - Separação entre catálogo e estoque

O cadastro de peças e insumos e o controle de suas quantidades
possuem responsabilidades distintas.

O Catálogo será responsável pelas informações descritivas e
comerciais do item, enquanto o Estoque será responsável pelo
controle da quantidade existente.

## DD005 - Modelo simplificado de estoque

O MVP não realizará reserva de estoque para as Ordens de Serviço.

O estoque manterá uma única quantidade representando o saldo atual
de cada peça ou insumo.

A inclusão de uma peça ou insumo na Ordem de Serviço não altera
automaticamente o estoque.

## DD006 - Momento da baixa de estoque

A quantidade em estoque será reduzida quando uma peça ou insumo
for efetivamente registrado como consumido durante a execução
da Ordem de Serviço.

A simples inclusão do item no orçamento ou na Ordem de Serviço
não representa consumo de estoque.

## DD007 - Controle da quantidade em estoque

O estoque utilizará uma única quantidade para representar o saldo
atual de uma peça ou insumo.

Não serão mantidos, no MVP, conceitos separados de quantidade física,
quantidade reservada e quantidade disponível.

Não será permitido registrar consumo superior ao saldo atual.

## DD008 - Core Domain

O contexto de Ordem de Serviço será tratado como o Core Domain
do sistema.

Esse contexto concentra as principais regras do negócio relacionadas
ao atendimento do veículo, diagnóstico, orçamento, aprovação,
execução, finalização e entrega.

## DD009 - Separação entre agregados

Cliente, Veículo, Serviço e Ordem de Serviço possuirão ciclos de vida
e identidades independentes e serão tratados como Aggregate Roots
distintos.

Os relacionamentos entre agregados deverão ser realizados
preferencialmente por seus identificadores.

## DD010 - Dados do catálogo na Ordem de Serviço

Ao adicionar um serviço, peça ou insumo a uma Ordem de Serviço,
as informações comerciais necessárias àquele atendimento,
especialmente descrição e preço praticado, deverão ser registradas
no item da Ordem de Serviço.

Alterações posteriores no catálogo não deverão modificar os valores
já registrados em Ordens de Serviço existentes.

## DD011 - Responsabilidade sobre o estoque

As regras relacionadas ao controle da quantidade dos itens pertencem
ao contexto de Estoque.

A Ordem de Serviço poderá registrar o consumo de peças e insumos
durante sua execução, mas não será responsável por alterar diretamente
a quantidade mantida pelo Estoque.

## DD012 - Representação de peças e insumos

Peças e insumos serão representados por um único conceito de domínio.

A distinção entre os dois tipos será realizada através de um tipo
enumerado contendo os valores Peça e Insumo.

Essa decisão evita duplicação de estruturas e regras para conceitos
que, dentro do escopo atual do sistema, possuem o mesmo ciclo de vida
e comportamento.

## DD013 - Cálculo do tempo médio de execução

Para o MVP, o tempo médio de execução será calculado considerando
o intervalo entre o início da execução e a finalização das Ordens
de Serviço concluídas.

Serão consideradas no cálculo as Ordens de Serviço que possuam
DataInicioExecucao e DataFinalizacao registradas e estejam nos
estados Finalizada ou Entregue.

Ordens de Serviço ainda em execução ou que não tenham iniciado sua
execução não participarão do cálculo.

A métrica representa o tempo médio da fase de execução das Ordens
de Serviço e será apresentada em minutos.