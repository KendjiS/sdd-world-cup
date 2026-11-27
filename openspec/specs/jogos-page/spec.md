# jogos-page Specification

## Purpose

Define a página de Jogos do PortalCopa26: a consulta de todos os jogos oficiais da Copa do Mundo 2026, organizados por dia, com filtro por grupo e as informações do grupo e do estádio de cada partida, sempre com dados lidos do banco da aplicação.

## Requirements

### Requirement: Listagem dos jogos
A URL `/jogos` SHALL listar todos os jogos registrados no banco, da fase de grupos e do mata-mata. Os jogos SHALL ser agrupados pela data no horário de Brasília, com os dias em ordem cronológica e os jogos de cada dia em ordem de horário; jogos no mesmo horário SHALL seguir o número oficial do jogo.

#### Scenario: Todos os jogos com os dados oficiais
- **WHEN** um visitante abre `/jogos` com os dados oficiais carregados
- **THEN** a página exibe 104 jogos distribuídos em 34 dias, do jogo 1 (México x África do Sul, 11/06/2026) à final (19/07/2026)

#### Scenario: Jogo da madrugada agrupado pela data de Brasília
- **WHEN** a lista é exibida
- **THEN** Coreia do Sul x República Tcheca (gravado como 12/06/2026 02:00 UTC) aparece no dia 11/06/2026, às 23:00

### Requirement: Cabeçalho da página de jogos
A página SHALL exibir o título "Jogos" (ou "Jogos do Grupo X" quando filtrada) e um resumo com a quantidade de partidas exibidas, o período entre o primeiro e o último dia listados e a indicação de que os horários são de Brasília. O título da aba do navegador SHALL acompanhar o título da página.

#### Scenario: Resumo sem filtro
- **WHEN** a página é aberta sem filtro
- **THEN** o resumo mostra "104 partidas · 11 de junho a 19 de julho de 2026 · horários de Brasília"

#### Scenario: Resumo com filtro
- **WHEN** a página é aberta com o filtro do Grupo C
- **THEN** o título é "Jogos do Grupo C" e o resumo mostra "6 partidas · 13 de junho a 24 de junho de 2026 · horários de Brasília"

### Requirement: Título de cada dia
Cada dia SHALL ter um título com a data por extenso em português e a quantidade de jogos do dia ("1 jogo" ou "N jogos").

#### Scenario: Primeiro dia da Copa
- **WHEN** a lista sem filtro é exibida
- **THEN** o primeiro dia tem o título "Quinta-feira, 11 de junho de 2026" e a contagem "2 jogos"

### Requirement: Cartão de jogo na página de jogos
Cada jogo SHALL mostrar o selo "Agendado"; para mandante e visitante, a bandeira, o nome e o código FIFA; o horário em Brasília (HH:mm); o grupo ou, no mata-mata, o rótulo da rodada; o estádio e a cidade; o país-sede e a capacidade do estádio em pt-BR; e o número oficial do jogo. Vaga de mata-mata indefinida SHALL mostrar a descrição da vaga no lugar do nome, sem bandeira.

#### Scenario: Informações do estádio
- **WHEN** o jogo 1 (México x África do Sul) é exibido
- **THEN** o cartão mostra "Estádio Azteca · Cidade do México" e "México · Capacidade 87.523 · Jogo 1"

#### Scenario: Jogo de mata-mata sem seleções definidas
- **WHEN** a final é exibida
- **THEN** o cartão mostra o rótulo "Final" no lugar do grupo e "Venc. Semifinal 1" e "Venc. Semifinal 2" no lugar dos nomes das seleções, sem bandeira

### Requirement: Filtro por grupo
A página SHALL oferecer as opções "Todos" e "Grupo X" para cada grupo registrado no banco, em ordem alfabética. Escolher um grupo SHALL exibir apenas os jogos daquele grupo; "Todos" SHALL exibir todos os jogos. O grupo escolhido SHALL ficar na URL no parâmetro `grupo`, sem diferenciar maiúsculas de minúsculas, e a opção ativa SHALL ficar destacada.

#### Scenario: Filtrar por um grupo
- **WHEN** o visitante clica em "Grupo C"
- **THEN** o navegador navega para `/jogos?grupo=C`, a página exibe os 6 jogos do Grupo C e a opção "Grupo C" fica destacada

#### Scenario: Link compartilhado com letra minúscula
- **WHEN** um visitante abre `/jogos?grupo=c`
- **THEN** a página exibe os jogos do Grupo C

#### Scenario: Voltar para todos
- **WHEN** o visitante clica em "Todos"
- **THEN** o navegador navega para `/jogos` e a página exibe todos os jogos

#### Scenario: Parâmetro de grupo vazio
- **WHEN** um visitante abre `/jogos?grupo=` (vazio ou só com espaços)
- **THEN** a página exibe todos os jogos, sem aviso, com "Todos" destacado

#### Scenario: Link da navegação destacado com filtro
- **WHEN** a página é aberta com o filtro do Grupo C
- **THEN** o link "Jogos" da barra de navegação continua destacado como página ativa

#### Scenario: Grupo inexistente
- **WHEN** um visitante abre `/jogos?grupo=Z`
- **THEN** a página exibe o aviso "O grupo "Z" não existe. Exibindo todos os jogos." e lista todos os jogos, com "Todos" destacado

### Requirement: Informações do grupo filtrado
Quando um grupo estiver filtrado, a página SHALL exibir, antes da lista, um painel com o nome do grupo, as suas seleções com bandeira (o cabeça de chave primeiro e marcado como "Cabeça de chave"), a quantidade de jogos e a quantidade e os nomes dos estádios onde o grupo joga, com a cidade de cada um. Sem filtro, o painel SHALL ficar oculto.

#### Scenario: Painel do Grupo C
- **WHEN** a página é aberta com o filtro do Grupo C
- **THEN** o painel mostra Brasil (cabeça de chave), Marrocos, Haiti e Escócia, "6 jogos" e "5 estádios"

#### Scenario: Sem filtro
- **WHEN** a página é aberta sem filtro
- **THEN** o painel de informações do grupo não é exibido

### Requirement: Link para os grupos
A página SHALL exibir o botão "Ver Grupos", que leva a `/grupos`.

#### Scenario: Botão Ver Grupos
- **WHEN** o visitante clica em "Ver Grupos"
- **THEN** o navegador navega para `/grupos`
