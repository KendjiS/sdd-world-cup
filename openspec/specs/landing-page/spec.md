# landing-page Specification

## Purpose

Define a página inicial do PortalCopa26: a porta de entrada do portal, que apresenta a Copa do Mundo 2026 com os países-sede, números do torneio, próximos jogos, o Top 10 do Ranking FIFA e a chamada para o simulador, sempre com dados lidos do banco da aplicação.

## Requirements

### Requirement: Composição da página inicial
A URL raiz (`/`) SHALL exibir, nesta ordem: a seção hero, as estatísticas da Copa, os próximos jogos e, lado a lado em telas largas, o gráfico do Ranking FIFA e o painel do simulador. O título da aba do navegador SHALL identificar o portal e a Copa do Mundo FIFA 2026.

#### Scenario: Seções presentes
- **WHEN** um visitante abre `/`
- **THEN** a página exibe as seções Hero, Estatísticas, "Próximos Jogos", "Ranking FIFA" e "Simulador", nessa ordem

#### Scenario: Ranking e simulador empilham em telas estreitas
- **WHEN** a página é exibida em uma janela com menos de 992 px de largura
- **THEN** o painel do simulador aparece abaixo do gráfico do Ranking FIFA, em vez de ao lado

### Requirement: Seção hero
A seção hero SHALL exibir o selo "Copa do Mundo FIFA", o título "O Mundo inteiro no seu portal", uma descrição do portal, o logo da Copa do Mundo FIFA 2026 e dois botões: "Abrir Simulador", que leva a `/simulador`, e "Ver Jogos", que leva a `/jogos`.

#### Scenario: Botões de ação
- **WHEN** o visitante clica em "Abrir Simulador"
- **THEN** o navegador navega para `/simulador`
- **WHEN** o visitante clica em "Ver Jogos"
- **THEN** o navegador navega para `/jogos`

### Requirement: Países-sede na seção hero
A seção hero SHALL exibir um cartão para cada país-sede, na ordem Estados Unidos, Canadá e México. Cada cartão SHALL ter a bandeira do país, o nome e o texto "País Sede · N estádios", em que N é a quantidade de estádios desse país registrada no banco.

#### Scenario: Quantidade de estádios por país
- **WHEN** a página inicial é exibida com os dados oficiais carregados
- **THEN** os cartões mostram "Estados Unidos · País Sede · 11 estádios", "Canadá · País Sede · 2 estádios" e "México · País Sede · 3 estádios"

### Requirement: Estatísticas da Copa
A página SHALL exibir quatro indicadores com o total de seleções, grupos, jogos e estádios. Os totais SHALL ser calculados a partir dos registros do banco, e não fixados na página.

#### Scenario: Totais com os dados oficiais
- **WHEN** a página inicial é exibida com os dados oficiais carregados
- **THEN** os indicadores mostram 48 Seleções, 12 Grupos, 104 Jogos e 16 Estádios

#### Scenario: Totais acompanham o banco
- **WHEN** o banco contém um número diferente de registros de alguma dessas entidades
- **THEN** o indicador correspondente mostra a quantidade existente no banco

### Requirement: Data de referência dos próximos jogos
Os próximos jogos SHALL ser calculados a partir de uma data de referência lida da configuração da aplicação e interpretada no horário de Brasília. Quando a configuração não informar uma data válida, a data atual em Brasília SHALL ser usada.

#### Scenario: Data configurada
- **WHEN** a data de referência configurada é 05/06/2026
- **THEN** os próximos jogos são os de 11/06/2026 e 12/06/2026, os dois primeiros dias com partidas a partir dessa data

#### Scenario: Data ausente na configuração
- **WHEN** a configuração não informa a data de referência
- **THEN** a data atual no horário de Brasília é usada como referência

### Requirement: Seleção dos próximos jogos
A seção "Próximos Jogos" SHALL listar todos os jogos dos dois primeiros dias com partidas cuja data, no horário de Brasília, seja igual ou posterior à data de referência. Os jogos SHALL ser agrupados por dia, com os dias em ordem cronológica e os jogos de cada dia em ordem de horário. Quando não houver jogos a partir da data de referência, a seção SHALL exibir a mensagem "Nenhum jogo programado a partir da data de referência."

#### Scenario: Agrupamento por dia
- **WHEN** a data de referência é 05/06/2026
- **THEN** a seção exibe o grupo "Quinta-feira, 11 de junho de 2026" com 2 jogos: México x África do Sul às 16:00 e Coreia do Sul x República Tcheca às 23:00
- **AND** exibe o grupo "Sexta-feira, 12 de junho de 2026" com 2 jogos: Canadá x Bósnia e Herzegovina às 16:00 e Estados Unidos x Paraguai às 22:00

#### Scenario: Dia definido pelo horário de Brasília
- **WHEN** um jogo acontece à 01:00 de 14/06/2026 no horário de Brasília (Austrália x Turquia)
- **THEN** ele é considerado jogo do dia 14/06/2026, e não de 13/06/2026

#### Scenario: Sem jogos futuros
- **WHEN** a data de referência é posterior à final (19/07/2026)
- **THEN** a seção exibe a mensagem "Nenhum jogo programado a partir da data de referência."

### Requirement: Cartão de jogo
Cada dia SHALL mostrar seu título por extenso em português e a quantidade de jogos ("1 jogo" ou "N jogos"). Cada jogo SHALL mostrar, para mandante e visitante, a bandeira, o nome e o código FIFA, além do horário em Brasília (HH:mm), o grupo (quando houver) ou, em jogos de mata-mata, o rótulo da rodada, o estádio e a cidade. Em jogos de mata-mata com seleções ainda não definidas, a descrição da vaga SHALL ocupar o lugar do nome, sem bandeira.

#### Scenario: Jogo da fase de grupos
- **WHEN** o jogo México x África do Sul é exibido
- **THEN** o cartão mostra México (MEX) e África do Sul (RSA) com suas bandeiras, o horário 16:00, o grupo A e "Estádio Azteca · Cidade do México"

#### Scenario: Rótulo da rodada no mata-mata
- **WHEN** o jogo da final é exibido
- **THEN** o cartão mostra o rótulo "Final" no lugar do grupo

#### Scenario: Vaga de mata-mata indefinida
- **WHEN** um jogo de mata-mata sem seleção definida é exibido
- **THEN** o cartão mostra a descrição da vaga (por exemplo, "Venc. Segundafase 1") no lugar do nome da seleção

#### Scenario: Link para todos os jogos
- **WHEN** o visitante clica em "Ver todos" na seção "Próximos Jogos"
- **THEN** o navegador navega para `/jogos`

### Requirement: Gráfico do Ranking FIFA Top 10
A seção "Ranking FIFA" SHALL exibir um gráfico de barras horizontais renderizado com Chart.js com as 10 primeiras posições do ranking FIFA do banco, em ordem de posição. Cada barra SHALL ter o rótulo "<posição>. <seleção>" e o comprimento igual à pontuação. As três primeiras barras SHALL usar ouro, prata e bronze, e as demais SHALL usar azul. A dica de cada barra SHALL mostrar a pontuação formatada no padrão pt-BR seguida de "pts".

#### Scenario: Top 10 com os dados oficiais
- **WHEN** a página inicial é exibida com os dados oficiais carregados
- **THEN** o gráfico tem 10 barras, a primeira "1. Argentina" com 1.877,72 pontos e a última "10. Alemanha" com 1.735,77 pontos

#### Scenario: Destaque do pódio
- **WHEN** o gráfico é renderizado
- **THEN** as barras de 1º, 2º e 3º lugar têm as cores ouro, prata e bronze, e as barras de 4º a 10º têm a cor azul

#### Scenario: Link para o ranking completo
- **WHEN** o visitante clica em "Ranking completo"
- **THEN** o navegador navega para `/ranking`

#### Scenario: Gráfico liberado ao sair da página
- **WHEN** o visitante sai da página inicial pela navegação do portal
- **THEN** a instância do gráfico é destruída, sem gráficos órfãos acumulados no navegador

### Requirement: Painel do simulador
A página SHALL exibir o painel "Simule a Copa!" com uma breve explicação do simulador, o botão "Iniciar Simulação", que leva a `/simulador`, e o botão "Ver Grupos", que leva a `/grupos`.

#### Scenario: Chamada para o simulador
- **WHEN** o visitante clica em "Iniciar Simulação"
- **THEN** o navegador navega para `/simulador`
- **WHEN** o visitante clica em "Ver Grupos"
- **THEN** o navegador navega para `/grupos`
