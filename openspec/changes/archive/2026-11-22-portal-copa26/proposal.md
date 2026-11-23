# Proposal

## Why

O protótipo HTML/CSS/JS do PortalCopa26 foi validado, mas os dados estão fixos em JavaScript e nada é persistido. Todas as funcionalidades previstas (Landing Page, Jogos, Grupos, Seleções, Ranking FIFA e Simulador) dependem da mesma base. Essa base é uma aplicação Blazor Web App em .NET 10, com um modelo de dados persistido em SQLite via EF Core e com os dados oficiais da Copa 2026 carregados automaticamente. Construir essa fundação agora evita que cada página futura reinvente acesso a dados, estrutura de pastas e carga inicial.

## What Changes

- Criação da solução `src/PortalCopa26/PortalCopa26.slnx` com um único projeto Blazor Web App (`src/PortalCopa26/PortalCopa26/`) em .NET 10, com Bootstrap 5.
- Estrutura inicial de pastas (`Components/`, `Models/`, `Services/`, `Data/`) organizada para permitir futura separação em camadas.
- Configuração do EF Core com SQLite, `PortalCopaDbContext`, migrations e aplicação automática do schema na inicialização.
- Entidades iniciais: `Grupo`, `Selecao`, `Jogador`, `Estadio`, `Jogo`, `RankingFifa`, `Simulacao` e `SimulacaoJogo`. `Estadio` foi acrescentada à lista original para atender "Informações do estádio".
- Seed Data idempotente com os dados da Copa 2026, gerado por um script versionado em `tools/seed-generator/` a partir das fontes do protótipo (`prototipo/fontes/*.txt`): 12 grupos, 48 seleções, 16 estádios, os 104 jogos (72 da fase de grupos, 16 confrontos reais da Segunda Fase e as fases seguintes com placeholders), elencos e ranking FIFA.
- Registro dos serviços na injeção de dependência nativa do ASP.NET Core por meio de métodos de extensão.
- Chart.js disponibilizado como asset estático local, preparando o futuro gráfico do Ranking FIFA via JSInterop.

**Fora do escopo:** implementação das páginas Landing, Jogos, Grupos, Seleções, Ranking FIFA e Simulador; cálculo de classificação e lógica de simulação; integração com APIs externas; área administrativa; autenticação.

## Capabilities

### New Capabilities
- `app-foundation`: estrutura da solução e do projeto, hospedagem Blazor Web App, organização de pastas, registro de dependências e assets compartilhados (Bootstrap, Chart.js).
- `data-model`: modelo de dados persistido em SQLite (entidades, relacionamentos, restrições), criação e atualização do schema e persistência das simulações.
- `seed-data`: carga inicial idempotente dos dados oficiais da Copa 2026 (grupos, seleções, estádios, jogos, jogadores, ranking FIFA).

### Modified Capabilities
<!-- Nenhuma: não há specs existentes no projeto. -->

## Impact

- **Código:** cria todo o conteúdo de `src/`, que hoje está vazio. Também cria `tools/` na raiz do workspace, com os scripts de apoio que não são código da aplicação: o gerador do seed e o verificador do banco.
- **Dependências:** `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design` e a ferramenta local `dotnet-ef`, todos numa mesma versão exata 10.0.x; Chart.js como arquivo estático; Python 3 (já instalado) para os scripts de `tools/`.
- **Ambiente (pré-requisito):** a máquina tem hoje apenas o SDK .NET 11 e o runtime ASP.NET Core 11. É preciso **instalar o SDK .NET 10** antes da implementação. A solução fixa esse SDK com um `global.json`.
- **Dados:** cria o arquivo local `src/PortalCopa26/PortalCopa26/App_Data/portalcopa26.db`, fora do controle de versão. O caminho é resolvido a partir da pasta do projeto, independentemente de onde a aplicação é executada. As simulações gravadas nele sobrevivem a reinícios.
- **Fontes:** os dados do seed derivam de `C:\Users\kendj\wrk\world-cup\prototipo\fontes\`. Os códigos FIFA e as participações em Copas dos jogadores vêm de `prototipo/js/site.js`.
