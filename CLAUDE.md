# PortalCopa26

## Projeto

Portal informativo da Copa do Mundo 2026 focado em consulta de jogos, grupos, seleções, ranking FIFA e simulação de resultados.

---

## Tecnologias

- .NET 10 (SDK .NET 10 obrigatório; a versão do SDK é fixada por `global.json` em `src/PortalCopa26/`, pois a máquina também tem o SDK .NET 11)
- Blazor Web App
- EF Core
- SQLite
- Bootstrap 5
- JSinterop
- Chart.js

---

## Arquitetura

A aplicação será desenvolvida inicialmente em um único projeto Blazor Web App.

### Organização

- Components (inclui `Components/Pages` para as páginas e `Components/Layout` para os layouts, conforme a convenção do template Blazor Web App)
- Models
- Services
- Data

O código deve ser organizado de forma que permita futura migração para uma arquitetura em camadas sem grandes alterações.

---

## Escopo

### Landing Page

Página inicial contendo:

- Hero Section
- Países-sede
- Próximos jogos
- Ranking FIFA : Grafico de barras com Chart.js
- Chamada para o simulador

### Jogos

- Listagem de jogos
- Ordenação por data
- Informações do grupo
- Informações do estádio

### Grupos

- Exibição dos grupos
- Classificação
- Estatísticas

### Seleções

- Informações das seleções
- Elencos
- Estatísticas dos jogadores : nome, posição , idade, gols marcados, participação em copas

### Ranking FIFA

- Exibição do ranking das seleções

### Simulador

- Simulação de resultados
- Atualização da classificação
- Simulação da fase de grupos
- Persistir a simulação

## Visualizações e Gráficos

A Landing Page deverá exibir um gráfico de barras com o Ranking FIFA.

- Utilizar Chart.js para renderização dos gráficos
- Integrar o Chart.js através de JSInterop
- O gráfico deverá exibir as principais seleções e sua pontuação no ranking FIFA
- O componente deve ser reutilizável para futuras visualizações estatísticas

---

## Persistência

A aplicação utilizará SQLite através do EF Core.

Além dos dados oficiais da Copa, o banco deverá armazenar:

- Simulações realizadas pelos usuários
- Resultados simulados dos jogos
- Classificações geradas a partir das simulações

As simulações devem permanecer disponíveis mesmo após o encerramento da aplicação.

As classificações geradas a partir das simulações serão modeladas e persistidas junto com o Simulador. A fundação da aplicação cria apenas `Simulacao` e `SimulacaoJogo`.

## Fora do Escopo

Não fazem parte da primeira versão:

- Área administrativa
- Autenticação
- Autorização
- Gestão de usuários
- Integração com APIs externas
- Atualização automática dos resultados

---

## Diretrizes de Desenvolvimento

- Utilizar async/await sempre que aplicável
- Utilizar injeção de dependência nativa do ASP.NET Core
- Criar componentes Blazor reutilizáveis
- Evitar duplicação de código
- Seguir princípios SOLID quando aplicável
- Utilizar EF Core como mecanismo de persistência
- Utilizar SQLite como banco de dados local
- Utilizar JSInterop apenas quando necessário
- Priorizar legibilidade e manutenção do código

---

## Dados Iniciais

Os dados serão carregados através de Seed Data.

### Fonte dos dados

Os dados oficiais do torneio estão nos arquivos `.txt` da pasta `fontes/` do protótipo (`C:\Users\kendj\wrk\world-cup\prototipo\fontes`), que são a fonte de verdade. Os principais são:

- `copa2026_cidades_sede_estadios.txt` e `copa2026_estadios.txt`: cidades-sede e estádios
- `copa2026_cabecas-chave.txt`: cabeças de chave
- `copa2026_grupos.txt`: grupos e seleções
- `copa2026_fases.txt`: fases do mundial
- `copa2026_jogos_primeira_fase.txt`: jogos da fase de grupos
- `copa2026_Jogos_Segunda_fase.txt`, `copa2026_jogos_oitavas.txt`, `copa2026_jogos_quartas.txt`, `copa2026_jogos_semifinal.txt`, `copa2026_jogo_terceiro_lugar.txt`, `copa2026_jogo_final.txt`: jogos do mata-mata
- `copa2026_selecoes_jogadores.txt`: elencos
- `copa2026_ranking_fifa.txt`: ranking FIFA
- `copa2026_regras_negocio.txt` e `Copa2026_Regra_Terceiros_Colocados.txt`: regras de negócio e classificação dos terceiros colocados

De `prototipo/js/site.js` são usados apenas os códigos FIFA das seleções e as participações em Copas dos jogadores. O restante desse arquivo está desatualizado. Os dados são convertidos em JSON por `tools/seed-generator/` e carregados pelo seeder da aplicação.

Ao implementar funcionalidades relacionadas ao torneio:

- Não gerar dados fictícios
- Não inventar confrontos
- Não criar grupos não definidos
- Utilizar exclusivamente os dados das fontes acima

### Dados previstos

- Seleções: 48
- Grupos: 12 (A a L)
- Estádios: 16
- Fases: 7
- Jogadores: elencos das 48 seleções
- Jogos: 104
- Ranking FIFA

### Fases do mundial

| Fase             | Times | Jogos | Datas              |
|------------------|-------|-------|--------------------|
| Fase de Grupos   | 48    | 72    | 11 jun – 2 jul     |
| Segunda Fase     | 32    | 16    | 28 jun – 3 jul     |
| Oitavas de Final | 16    | 8     | 4 jul – 9 jul      |
| Quartas de Final | 8     | 4     | 11 jul – 13 jul    |
| Semifinais       | 4     | 2     | 15 jul – 16 jul    |
| 3º Lugar         | 2     | 1     | 18 jul (Miami)     |
| Final            | 2     | 1     | 19 jul (Nova York) |

Total: 104 jogos. O arquivo `copa2026_fases.txt` informa "total de 102 jogos", mas a soma das suas próprias linhas é 104. Vale o total de 104.

### Imagens da FIFA

As bandeiras das seleções poderão ser obtidas pela API pública da FIFA, usando o código FIFA da seleção:

https://api.fifa.com/api/v3/picture/flags-sq-4/MEX

O logo da FIFA poderá ser obtido pela mesma API:

https://api.fifa.com/api/v3/picture/tournaments-sq-4/285023

## Estrutura Física

Todo o código-fonte da aplicação deverá ser criado dentro da pasta src/ localizada na raiz do workspace atual

Estrutura desejada:
src/
└── PortalCopa26
    ├── PortalCopa26.slnx
    └── PortalCopa26/

Utilizar o formato de solução com extensão .slnx para o arquivo da solução
