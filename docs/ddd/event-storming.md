# Event Storming

## Objetivo

O Event Storming foi utilizado para explorar os principais fluxos do
domínio da oficina mecânica, identificar eventos de domínio, comandos,
atores, políticas, regras de negócio, agregados candidatos e pontos que
necessitavam de maior investigação.

Foram modelados os fluxos relacionados à Ordem de Serviço e à gestão
de peças, insumos e estoque.

A versão visual dos Event Stormings encontra-se no arquivo:

- [Event Storming](event-storming.pdf)

## Event Storming - Ordem de Serviço

O fluxo de Ordem de Serviço contempla a criação e o acompanhamento
da Ordem de Serviço durante seu ciclo de vida.

Entre os principais elementos identificados estão:

- identificação do cliente;
- identificação do veículo;
- criação da Ordem de Serviço;
- registro dos serviços solicitados;
- início e conclusão do diagnóstico;
- identificação e registro das peças e insumos necessários;
- geração automática do orçamento;
- disponibilização do orçamento ao cliente;
- aprovação ou rejeição do orçamento;
- início da execução após aprovação;
- registro do consumo de peças e insumos;
- conclusão dos serviços;
- finalização da Ordem de Serviço;
- entrega do veículo.

Os estados da Ordem de Serviço considerados durante o processo são:

- Recebida;
- Em diagnóstico;
- Aguardando aprovação;
- Em execução;
- Finalizada;
- Entregue.

As alterações de status são consequências das ações realizadas
durante o ciclo da Ordem de Serviço.

### Aprovação do orçamento

Após a disponibilização do orçamento, o cliente poderá aprová-lo
ou rejeitá-lo.

A aprovação permite o início da execução da Ordem de Serviço,
mas não inicia a execução automaticamente.

A rejeição não autoriza o início da execução.

### Pontos de atenção

Durante o Event Storming foram identificadas questões que ainda não
possuem definição explícita nos requisitos do MVP.

Permanecem como pontos de atenção:

- definir o comportamento da Ordem de Serviço após a rejeição
  de um orçamento;
- definir se um orçamento já disponibilizado ao cliente poderá
  ser alterado.

Esses pontos não impedem a implementação dos requisitos obrigatórios
da primeira versão do sistema e poderão ser refinados conforme
necessidade do domínio.

## Event Storming - Gestão de Peças, Insumos e Estoque

O segundo fluxo modelado representa as operações necessárias para
o controle simplificado do estoque da oficina.

O MVP trabalhará com uma única quantidade representando o saldo
atual de cada peça ou insumo.

Não será utilizado mecanismo de reserva de estoque.

O fluxo contempla:

- cadastro de peça ou insumo;
- registro de entrada de estoque;
- atualização da quantidade em estoque;
- registro de consumo durante a execução da Ordem de Serviço;
- atualização da quantidade após o consumo.

O consumo somente poderá ser registrado quando existir quantidade
suficiente em estoque.

A simples inclusão de uma peça ou insumo na Ordem de Serviço ou
no orçamento não altera a quantidade existente em estoque.

## Relação entre os fluxos

Os Event Stormings de Ordem de Serviço e Estoque se relacionam
durante a execução da Ordem de Serviço.

Quando uma peça ou insumo é efetivamente utilizado, a Ordem de Serviço
registra o consumo e o contexto de Estoque é responsável por validar
e atualizar a quantidade existente.

Essa separação preserva as responsabilidades identificadas nos
Bounded Contexts de Ordem de Serviço e Estoque.

## Artefatos relacionados

- [Linguagem Ubíqua](linguagem-ubiqua.md)
- [Regras de Negócio](regras-negocio.md)
- [Decisões de Domínio](decisoes-dominio.md)
- [Context Map](context-map.md)
- [Diagramas DDD](diagramas.pdf)