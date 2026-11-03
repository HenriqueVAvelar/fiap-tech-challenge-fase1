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

## Documento

CPF ou CNPJ de um cliente.

É o identificador utilizado pelo cliente e pela oficina para localizar
seus dados e suas Ordens de Serviço, inclusive para aprovar ou rejeitar
um orçamento. Pode ser informado com ou sem formatação.

## Placa

Identificação do veículo, composta por letras e números.

Cada placa pertence a um único veículo cadastrado e é utilizada para
associar a Ordem de Serviço ao veículo e para identificar a Ordem de
Serviço correta quando o cliente possui mais de uma aguardando decisão.

## Ordem de Serviço

Registro responsável por representar o atendimento de um veículo
pela oficina.

A Ordem de Serviço registra o veículo, cliente, serviços solicitados,
peças utilizadas, orçamento e andamento do atendimento.

## Item de Serviço

Serviço incluído em uma Ordem de Serviço.

Registra o serviço de origem, sua descrição e o valor praticado no
momento da inclusão.

## Item de Peça/Insumo

Peça ou insumo incluído em uma Ordem de Serviço.

Registra o item de catálogo de origem, o nome, o tipo, a quantidade e o
valor unitário praticado no momento da inclusão. Também indica se o
item já foi consumido na execução.

## Preço Praticado

Valor de um serviço, peça ou insumo registrado no item da Ordem de
Serviço no momento em que foi incluído.

Alterações posteriores de preço no Catálogo não modificam Ordens de
Serviço existentes.

## Consumido

Situação de um Item de Peça/Insumo cujo uso foi registrado durante a
execução da Ordem de Serviço.

Um item só pode ser consumido uma vez.

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

As informações de identificação e preço pertencem ao Catálogo. A
quantidade existente pertence ao Estoque.

## Insumo

Material consumível utilizado durante a execução dos serviços.

## Catálogo

Conjunto das opções comerciais oferecidas pela oficina: serviços,
peças e insumos, com seus respectivos preços.

## Item de Catálogo

Peça ou insumo cadastrado no Catálogo.

Peças e insumos são o mesmo conceito, diferenciado pelo Tipo.
Possui nome, preço, tipo, situação (ativo ou inativo) e descrição
opcional.

## Tipo

Classificação de um Item de Catálogo como Peça ou Insumo.

## Ativo / Inativo

Situação de um serviço ou Item de Catálogo.

Itens inativos permanecem cadastrados, mas não podem ser adicionados a
novas Ordens de Serviço.

## Estoque

Representa a disponibilidade das peças e insumos da oficina.

Cada registro de estoque está associado a um Item de Catálogo e mantém
uma única quantidade.

## Saldo

Quantidade atual de uma peça ou insumo em estoque.

É o valor utilizado para verificar se um consumo pode ser realizado.
Não há, no MVP, quantidade reservada ou disponível separada.

## Baixa

Redução direta da quantidade em estoque, realizada sobre o próprio
registro de estoque.

Difere do consumo registrado a partir de uma Ordem de Serviço, mas em
ambos os casos a quantidade não pode ser superior ao saldo.

## Diagnóstico

Etapa da Ordem de Serviço na qual a oficina analisa o veículo e
identifica os serviços e peças necessários.

## Conclusão do Diagnóstico

Registro de que a análise do veículo foi finalizada.

Não altera o status da Ordem de Serviço, mas é necessária para que o
orçamento possa ser gerado.

## Orçamento

Valor calculado para execução da Ordem de Serviço.

O orçamento é composto pelos serviços e pelas peças/insumos incluídos
na Ordem de Serviço.

Além do valor, o orçamento possui um status próprio e registra as datas
de geração, disponibilização e decisão.

## Geração do Orçamento

Cálculo e registro do orçamento a partir dos serviços e das
peças/insumos da Ordem de Serviço.

Exige diagnóstico concluído e ao menos um serviço. Após a geração, não é
possível adicionar novos serviços nem peças e insumos.

## Disponibilização do Orçamento

Ato de colocar o orçamento à disposição do cliente para análise.

Altera o status da Ordem de Serviço para Aguardando aprovação.

## Valor do Orçamento

O orçamento apresenta três valores:

- valor dos serviços;
- valor das peças e insumos;
- valor total, que é a soma dos dois anteriores.

## Status do Orçamento

Representa a situação do orçamento, independente do status da Ordem de
Serviço.

Os estados definidos são:

- Pendente;
- Aprovado;
- Rejeitado.

## Aprovação do Orçamento

Decisão do cliente autorizando a execução dos serviços apresentados
no orçamento.

## Rejeição do Orçamento

Decisão do cliente de não autorizar a execução dos serviços
apresentados em um orçamento.

Um orçamento rejeitado não permite o início da execução
da Ordem de Serviço.

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

A aprovação ou a rejeição do orçamento altera o Status do Orçamento,
mas não o status da Ordem de Serviço, que permanece em Aguardando
aprovação até o início da execução.

## Início da Execução

Momento em que a oficina começa a executar os serviços.

Só pode ocorrer depois da aprovação do orçamento e altera o status da
Ordem de Serviço para Em execução.

## Finalização

Conclusão da execução dos serviços pela oficina.

Altera o status da Ordem de Serviço para Finalizada.

## Entrega

Devolução do veículo ao cliente.

Altera o status da Ordem de Serviço para Entregue.

## Tempo Médio de Execução

Média, em minutos, do intervalo entre o início da execução e a
finalização das Ordens de Serviço Finalizadas ou Entregues.

Ordens de Serviço sem execução iniciada ou ainda em execução não
participam do cálculo.

## Quantidade

Quantidade de uma peça ou insumo existente no estoque.

É aumentada pelo registro de entrada e reduzida pelo consumo
ocorrido durante a execução de uma Ordem de Serviço.

A quantidade não poderá assumir valor negativo.

Também não é permitido consumir uma quantidade superior à existente.

## Entrada de Estoque

Operação que registra o recebimento de peças ou insumos pela oficina,
aumentando a quantidade existente em estoque.

## Consumo de Estoque

Operação que registra a utilização de uma peça ou insumo durante a
execução de uma Ordem de Serviço, reduzindo a quantidade existente
em estoque.

A inclusão de uma peça ou insumo na Ordem de Serviço não é consumo.
O consumo só ocorre quando o uso é registrado com a Ordem de Serviço em
execução, e o registro é tudo ou nada: se algum item for inválido, já
consumido ou sem saldo, nenhum consumo é efetivado.

## Usuário

Pessoa que acessa o sistema da oficina mediante autenticação.

## Perfil

Papel do usuário, que determina as operações que ele pode executar.

Os perfis definidos são:

- Administrador;
- Mecânico.

## Bounded Context

Limite dentro do qual os termos deste documento têm um significado
específico e consistente.

Os contextos identificados são Cadastro, Catálogo, Ordem de Serviço,
Estoque e Identidade e Acesso. Veja o [Context Map](context-map.md).

## Core Domain

Parte do domínio de maior valor para o negócio. Neste sistema, é o
contexto de Ordem de Serviço.

## Agregado

Conjunto de objetos tratado como uma unidade de consistência, acessado
por meio de uma única entidade raiz.

A Ordem de Serviço é a raiz de um agregado que contém seus itens de
serviço, itens de peça/insumo e o orçamento.

## Correspondência entre o domínio e o código

| Termo do domínio | Nome no código |
|---|---|
| Cliente | `Cliente` |
| Veículo | `Veiculo` |
| Documento | `Documento` (objeto de valor) |
| Placa | `Placa` (objeto de valor) |
| Serviço | `Servico` |
| Item de Catálogo (peça ou insumo) | `Catalogo` |
| Tipo | `TipoConsumivel` |
| Estoque | `Estoque` |
| Ordem de Serviço | `OrdemServico` |
| Item de Serviço | `ItemServico` |
| Item de Peça/Insumo | `ItemPecaInsumo` |
| Orçamento | `Orcamento` |
| Status da Ordem de Serviço | `StatusOrdemServico` |
| Status do Orçamento | `StatusOrcamento` |
| Usuário | `Usuario` |
| Perfil | `PerfilUsuario` |
