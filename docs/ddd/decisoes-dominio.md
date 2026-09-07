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
comerciais do item, enquanto o Estoque será responsável pelas
quantidades físicas, reservadas e disponíveis.

## DD005 - Momento da reserva de estoque

A reserva das peças e insumos necessários para uma Ordem de Serviço
será realizada após a aprovação do orçamento.

A inclusão de um item durante o diagnóstico não representará
consumo nem reserva automática de estoque.

## DD006 - Momento da baixa de estoque

A baixa física do estoque ocorrerá quando a peça ou insumo for
registrado como consumido durante a execução da Ordem de Serviço.

## DD007 - Disponibilidade de estoque

A quantidade disponível será calculada considerando a quantidade
física menos a quantidade reservada.

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

As regras relativas às quantidades físicas, reservadas e disponíveis
pertencem ao contexto de Estoque.

A Ordem de Serviço poderá solicitar operações de reserva e consumo,
mas não será responsável por alterar diretamente os saldos do estoque.