## Purpose

Define o modelo de dados persistido do PortalCopa26 em SQLite: as entidades da Copa 2026 (grupos, seleções, jogadores, estádios, jogos, ranking FIFA) e das simulações, seus relacionamentos e restrições, e como o banco é criado e mantido entre execuções.

## ADDED Requirements

### Requirement: Banco de dados local SQLite
A aplicação SHALL persistir seus dados em um arquivo de banco SQLite local, cujo caminho é definido por configuração. O arquivo SHALL ficar fora do controle de versão.

#### Scenario: Banco criado no primeiro início
- **GIVEN** que o arquivo de banco configurado não existe
- **WHEN** a aplicação é iniciada
- **THEN** o arquivo de banco é criado no caminho configurado com o schema completo

#### Scenario: Caminho configurável
- **WHEN** a connection string configurada aponta para outro arquivo
- **THEN** a aplicação lê e grava nesse arquivo

#### Scenario: Caminho relativo independe do diretório de execução
- **GIVEN** uma connection string com caminho relativo
- **WHEN** a aplicação é iniciada a partir de diretórios de trabalho diferentes
- **THEN** ela sempre usa o mesmo arquivo de banco, resolvido a partir da pasta raiz da aplicação

### Requirement: Evolução do schema por migrations
O schema do banco SHALL ser definido por migrations versionadas e aplicado automaticamente na inicialização da aplicação, sem perda dos dados existentes.

#### Scenario: Banco existente é atualizado
- **GIVEN** um banco criado por uma versão anterior com migrations pendentes
- **WHEN** a aplicação é iniciada
- **THEN** as migrations pendentes são aplicadas
- **AND** os dados existentes, incluindo simulações, são preservados

### Requirement: Grupos
O sistema SHALL armazenar os grupos da fase de grupos, cada um identificado por um código único de uma letra (A a L).

#### Scenario: Código de grupo único
- **WHEN** se tenta gravar um segundo grupo com um código já existente
- **THEN** a gravação é rejeitada

### Requirement: Seleções
O sistema SHALL armazenar cada seleção participante com código FIFA de três letras (único), nome, técnico, grupo e indicação de cabeça de chave. Cada seleção SHALL pertencer a exatamente um grupo.

#### Scenario: Código FIFA único
- **WHEN** se tenta gravar uma segunda seleção com um código FIFA já existente
- **THEN** a gravação é rejeitada

#### Scenario: Bandeira derivada do código FIFA
- **WHEN** a bandeira de uma seleção é solicitada
- **THEN** o endereço retornado é `https://api.fifa.com/api/v3/picture/flags-sq-4/{CodigoFifa}`

### Requirement: Jogadores
O sistema SHALL armazenar o elenco de cada seleção. Cada jogador SHALL ter nome, posição (Goleiro, Defensor, Meio-campista ou Atacante), idade, gols marcados pela seleção e número de participações em Copas, e SHALL pertencer a exatamente uma seleção.

#### Scenario: Jogador vinculado à seleção
- **WHEN** os jogadores de uma seleção são consultados
- **THEN** apenas os jogadores daquela seleção são retornados, cada um com nome, posição, idade, gols e participações em Copas

### Requirement: Estádios
O sistema SHALL armazenar os estádios da Copa com nome, cidade, país-sede (Canadá, Estados Unidos ou México) e capacidade.

#### Scenario: Estádio de um jogo
- **WHEN** um jogo é consultado
- **THEN** o estádio, a cidade e o país onde ele será disputado estão disponíveis

### Requirement: Jogos
O sistema SHALL armazenar cada jogo com número sequencial único, fase, data e hora, e estádio. Jogos da fase de grupos SHALL referenciar seu grupo. O placar oficial SHALL ser opcional.

#### Scenario: Jogo da fase de grupos
- **WHEN** um jogo da fase de grupos é consultado
- **THEN** ele informa seu grupo, as duas seleções, data e hora, estádio e placar (vazio se não disputado)

#### Scenario: Ordenação por data
- **WHEN** os jogos são consultados ordenados por data e hora
- **THEN** a ordem retornada é cronológica

### Requirement: Jogos de mata-mata com confronto indefinido
Jogos de mata-mata SHALL permitir seleções mandante e visitante ausentes. Nesse caso SHALL guardar uma descrição textual de cada vaga (ex.: "Venc. Segundafase 1").

#### Scenario: Confronto ainda não definido
- **WHEN** uma vaga de um jogo de mata-mata ainda não tem seleção definida
- **THEN** o jogo continua válido e expõe a descrição textual daquela vaga

### Requirement: Ranking FIFA
O sistema SHALL armazenar o ranking FIFA com posição, nome da seleção e pontuação. Entradas de seleções participantes SHALL referenciar a seleção correspondente. Entradas de seleções não classificadas para a Copa SHALL ser permitidas.

#### Scenario: Ranking ordenado
- **WHEN** o ranking é consultado ordenado por posição
- **THEN** cada entrada informa posição, nome da seleção e pontuação, com a seleção participante vinculada quando houver

### Requirement: Persistência de simulações
O sistema SHALL armazenar simulações nomeadas, com data de criação e de última atualização. Cada simulação SHALL guardar no máximo um resultado simulado (gols de cada lado) por jogo. As simulações SHALL permanecer disponíveis após o encerramento e reinício da aplicação.

#### Scenario: Simulação sobrevive ao reinício
- **GIVEN** que uma simulação com resultados simulados foi gravada
- **WHEN** a aplicação é encerrada e iniciada novamente
- **THEN** a simulação e todos os seus resultados simulados continuam disponíveis

#### Scenario: Um resultado por jogo em cada simulação
- **WHEN** se tenta gravar um segundo resultado simulado para o mesmo jogo na mesma simulação
- **THEN** a gravação é rejeitada

#### Scenario: Excluir simulação remove seus resultados
- **WHEN** uma simulação é excluída
- **THEN** todos os seus resultados simulados também são removidos
- **AND** os jogos oficiais não são afetados

#### Scenario: Jogos oficiais protegidos
- **WHEN** se tenta excluir um jogo que possui resultados simulados
- **THEN** a exclusão é rejeitada
