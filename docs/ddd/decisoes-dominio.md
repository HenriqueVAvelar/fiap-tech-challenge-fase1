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