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