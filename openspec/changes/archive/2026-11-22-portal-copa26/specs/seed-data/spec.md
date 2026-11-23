## Purpose

Define a carga inicial dos dados oficiais da Copa do Mundo 2026: grupos, seleções, estádios, jogos, elencos e ranking FIFA. Ela garante que a aplicação tenha um conjunto de dados completo e consistente desde o primeiro início, sem cadastro manual e sem integração com APIs externas.

## ADDED Requirements

### Requirement: Carga automática dos dados oficiais
Na inicialização, a aplicação SHALL popular o banco com os dados oficiais da Copa 2026 sempre que eles ainda não estiverem presentes. A carga SHALL usar apenas dados distribuídos com a aplicação, sem chamadas a serviços externos.

#### Scenario: Primeiro início com banco vazio
- **GIVEN** um banco recém-criado sem dados
- **WHEN** a aplicação é iniciada
- **THEN** o banco passa a conter 12 grupos (A a L), 48 seleções, 16 estádios, 104 jogos, os elencos das 48 seleções e as 98 entradas do ranking FIFA

#### Scenario: Funciona sem acesso à internet
- **WHEN** a aplicação é iniciada sem conexão de rede em um banco vazio
- **THEN** a carga inicial é concluída com sucesso

### Requirement: Carga idempotente
A carga inicial SHALL poder ser executada em todo início da aplicação sem duplicar nem alterar dados já carregados.

#### Scenario: Segundo início não duplica dados
- **GIVEN** que a carga inicial já foi executada
- **WHEN** a aplicação é iniciada novamente
- **THEN** a quantidade de grupos, seleções, estádios, jogos, jogadores e entradas do ranking permanece a mesma

### Requirement: Simulações preservadas pela carga
A carga inicial SHALL NOT criar, alterar ou excluir simulações nem resultados simulados.

#### Scenario: Simulações intactas após reinício
- **GIVEN** que existem simulações gravadas
- **WHEN** a aplicação é reiniciada e a carga inicial é avaliada
- **THEN** todas as simulações e seus resultados simulados permanecem inalterados

### Requirement: Seleções e grupos
Cada uma das 48 seleções SHALL ser carregada com código FIFA, nome em português, técnico e grupo conforme o sorteio oficial. A primeira seleção de cada grupo SHALL ser marcada como cabeça de chave.

#### Scenario: Composição de um grupo
- **WHEN** o grupo C é consultado após a carga
- **THEN** ele contém Brasil (BRA, cabeça de chave), Marrocos (MAR), Haiti (HAI) e Escócia (SCO)

#### Scenario: Técnico carregado
- **WHEN** a seleção do Brasil é consultada após a carga
- **THEN** seu técnico é Carlo Ancelotti

#### Scenario: Doze cabeças de chave
- **WHEN** as seleções cabeças de chave são contadas após a carga
- **THEN** há exatamente 12, uma por grupo

### Requirement: Estádios
Os 16 estádios das cidades-sede SHALL ser carregados com nome, cidade, país-sede e capacidade.

#### Scenario: Estádio do jogo de abertura
- **WHEN** o estádio da Cidade do México é consultado após a carga
- **THEN** ele é o Estádio Azteca, no México, com capacidade de 87.523 lugares

### Requirement: Jogos da fase de grupos
Os 72 jogos da fase de grupos SHALL ser carregados com grupo, seleções mandante e visitante, data e hora, e estádio, sem placar.

#### Scenario: Jogo de abertura
- **WHEN** o jogo número 1 é consultado após a carga
- **THEN** ele é México x África do Sul, pelo grupo A, em 11/06/2026 às 16:00 (horário de Brasília), no Estádio Azteca

#### Scenario: Seis jogos por grupo
- **WHEN** os jogos de qualquer grupo são contados após a carga
- **THEN** há exatamente 6 jogos, cobrindo todos os confrontos entre as 4 seleções do grupo

### Requirement: Jogos do mata-mata
Os 32 jogos do mata-mata SHALL ser carregados: 16 da Segunda Fase, 8 das oitavas, 4 das quartas, 2 semifinais, disputa de 3º lugar e final. Cada um terá data, hora, estádio e rótulo. Os 16 jogos da Segunda Fase SHALL ter as seleções definidas nas fontes. Os demais SHALL usar descrições de vaga.

#### Scenario: Confronto definido na Segunda Fase
- **WHEN** o jogo "Segundafase 9" é consultado após a carga
- **THEN** ele é Brasil x Japão, em 29/06/2026 às 14:00 (horário de Brasília), em Houston

#### Scenario: Final com vagas indefinidas
- **WHEN** a final é consultada após a carga
- **THEN** ela não tem seleções definidas
- **AND** suas vagas são descritas como "Venc. Semifinal 1" e "Venc. Semifinal 2"
- **AND** ela é disputada em 19/07/2026 em Nova York/Nova Jersey

### Requirement: Elencos
Os jogadores de cada seleção SHALL ser carregados com nome, posição, idade e gols marcados pela seleção. As participações em Copas SHALL ser carregadas quando disponíveis nas fontes e SHALL valer 0 quando não houver informação.

#### Scenario: Jogador com dados completos
- **WHEN** o elenco da Argentina é consultado após a carga
- **THEN** ele inclui Lionel Messi, Atacante, 38 anos, 20 gols

#### Scenario: Participações em Copas sem informação
- **WHEN** um jogador sem participações informadas nas fontes é consultado
- **THEN** suas participações em Copas são 0

### Requirement: Ranking FIFA
As 98 entradas do ranking FIFA das fontes SHALL ser carregadas com posição, nome e pontuação. Entradas de seleções participantes SHALL ficar vinculadas à seleção correspondente.

#### Scenario: Líder do ranking
- **WHEN** a primeira posição do ranking é consultada após a carga
- **THEN** ela é Argentina, com 1877,72 pontos, vinculada à seleção ARG

#### Scenario: Seleção não classificada no ranking
- **WHEN** a entrada da Itália é consultada após a carga
- **THEN** ela está na 12ª posição e não está vinculada a nenhuma seleção participante

#### Scenario: Participantes presentes no ranking vinculadas
- **WHEN** as entradas do ranking vinculadas a seleções são contadas após a carga
- **THEN** cada seleção participante que aparece nas fontes do ranking está vinculada a exatamente uma entrada
- **AND** nenhuma seleção participante está vinculada a mais de uma entrada

#### Scenario: Participante ausente das fontes do ranking
- **GIVEN** uma seleção participante que não aparece nas fontes do ranking (ex.: Curaçao)
- **WHEN** sua posição no ranking é consultada após a carga
- **THEN** nenhuma entrada do ranking é retornada para ela e nenhum valor é inventado
