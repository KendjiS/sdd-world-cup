# Tasks

## 0. Pré-requisitos de ambiente

- [x] 0.1 Confirmar que o SDK .NET 10 está instalado (a instalação é feita pelo usuário); verificar que `dotnet --list-sdks` mostra um `10.0.x` e que `dotnet --list-runtimes` mostra `Microsoft.AspNetCore.App 10.0.x`. Se não mostrar, parar o apply e pedir a instalação.
- [x] 0.2 Confirmar que o Python 3 está disponível para os scripts de `tools/`; verificar com `python --version` (3.10 ou superior) e `python -c "import sqlite3"`.

## 1. Solução e projeto

- [x] 1.1 Criar `src/PortalCopa26/` e, dentro dela, o `global.json` com `dotnet new globaljson --sdk-version <10.0.x instalado> --roll-forward latestFeature`; verificar que `dotnet --version` executado em `src/PortalCopa26/` retorna `10.0.x`.
- [x] 1.2 Criar a solução com `dotnet new sln -n PortalCopa26 --format slnx` em `src/PortalCopa26/`; verificar que `src/PortalCopa26/PortalCopa26.slnx` existe.
- [x] 1.3 Criar o projeto com `dotnet new blazor -n PortalCopa26 -o PortalCopa26 -f net10.0 --interactivity Server --all-interactive false` e adicioná-lo à solução (`dotnet sln add PortalCopa26/PortalCopa26.csproj`); verificar que o `.csproj` declara `<TargetFramework>net10.0</TargetFramework>` e que `dotnet sln list` mostra apenas `PortalCopa26`.
- [x] 1.4 Criar as pastas `Models/`, `Services/`, `Data/Configurations/`, `Data/Seed/`, `Extensions/` e `App_Data/` (com `.gitkeep`); adicionar um `.gitignore` em `src/PortalCopa26/` que ignore `bin/`, `obj/` e `App_Data/*.db*`; verificar com listagem de diretórios.
- [x] 1.5 Rodar `dotnet build src/PortalCopa26/PortalCopa26.slnx` e `dotnet run --project src/PortalCopa26/PortalCopa26`; verificar build sem erros e que `GET /` responde 200 com a página do template (via `curl -k`).

## 2. Dependências

- [x] 2.1 Identificar a versão 10.0.x mais recente publicada de `Microsoft.EntityFrameworkCore.Sqlite` e adicioná-la, junto com `Microsoft.EntityFrameworkCore.Design`, com essa mesma versão exata (sem curingas); verificar que `dotnet list package` mostra ambas na mesma versão 10.0.x.
- [x] 2.2 Criar o manifesto de ferramentas (`dotnet new tool-manifest`) em `src/PortalCopa26/` e instalar `dotnet-ef` com a mesma versão exata de 2.1; verificar que `dotnet ef --version`, executado nessa pasta, mostra essa versão.

## 3. Modelo de domínio (`Models/`)

- [x] 3.1 Criar os enums `Posicao`, `Fase` e `PaisSede` (design D4); verificar compilação.
- [x] 3.2 Criar as entidades POCO `Grupo`, `Selecao` (com `BandeiraUrl` calculada a partir de `CodigoFifa`), `Jogador`, `Estadio`, `Jogo`, `RankingFifa`, `Simulacao` e `SimulacaoJogo`, com propriedades de navegação conforme D4; verificar compilação e que nenhum arquivo em `Models/` tem `using Microsoft.EntityFrameworkCore` nem `Microsoft.AspNetCore`.

## 4. Persistência (`Data/`)

- [x] 4.1 Criar `PortalCopaDbContext` com os 8 `DbSet` e `ApplyConfigurationsFromAssembly`; verificar compilação.
- [x] 4.2 Criar uma `IEntityTypeConfiguration<T>` por entidade em `Data/Configurations/` com:
  - índices únicos simples em `Grupo.Codigo`, `Selecao.CodigoFifa`, `Jogo.Numero`, `RankingFifa.Posicao`, `RankingFifa.SelecaoId` (sem filtro) e `SimulacaoJogo(SimulacaoId, JogoId)`;
  - delete behaviors: cascade Simulacao→SimulacaoJogo e Selecao→Jogador; restrict Grupo→Selecao, Grupo→Jogo, Selecao→Jogo, Estadio→Jogo e Jogo→SimulacaoJogo;
  - enums como string, `Pontos` como `decimal(7,2)`, `BandeiraUrl` ignorada e tamanhos máximos de texto.

  Verificar compilação.

## 5. Injeção de dependência e inicialização

- [x] 5.1 Adicionar a connection string `PortalCopa` (`Data Source=App_Data/portalcopa26.db`) em `appsettings.json`; verificar que o JSON é válido.
- [x] 5.2 Criar `Extensions/ServiceCollectionExtensions` com `AddPortalCopaData(IConfiguration, IWebHostEnvironment)` e `AddPortalCopaServices()`, e chamá-los no `Program.cs`. `AddPortalCopaData` usa `AddDbContextFactory` + SQLite e resolve um `DataSource` relativo contra `ContentRootPath` (design D6), criando o diretório do arquivo. Verificar que a aplicação inicia sem erro de DI e que o log mostra o caminho absoluto do banco dentro de `src/PortalCopa26/PortalCopa26/App_Data/`.
- [x] 5.3 Criar a migration `InitialCreate` com `dotnet ef migrations add InitialCreate -o Data/Migrations`; revisar o código gerado e verificar que contém as 8 tabelas, os índices únicos e as FKs com os delete behaviors de 4.2.
- [x] 5.4 Criar `Data/Seed/DatabaseInitializer` executando apenas `MigrateAsync` (o seeder é ligado na 7.1), e a extensão `app.InitializeDatabaseAsync()` chamada antes de `app.Run()`. Apagar `App_Data/portalcopa26.db` e rodar a aplicação duas vezes: uma de `src/PortalCopa26/PortalCopa26` e outra da raiz do workspace (`dotnet run --project ...`). Verificar que só existe um arquivo de banco, em `src/PortalCopa26/PortalCopa26/App_Data/`, com as 8 tabelas e `InitialCreate` em `__EFMigrationsHistory`.

## 6. Ferramenta de verificação do banco

- [x] 6.1 Criar `tools/db-check/verificar_banco.py` (Python, módulo `sqlite3` da biblioteca padrão), que recebe o caminho do `.db`. Ele imprime as contagens por tabela e oferece os subcomandos `contagens`, `simulacao-exemplo` (insere uma `Simulacao` com dois `SimulacaoJogo`) e `restricoes` (testa duplicidade de `SimulacaoJogo`, cascade da exclusão de `Simulacao` e bloqueio da exclusão de `Jogo` referenciado, numa transação revertida ao final). Verificar que `contagens` roda sobre o banco da 5.4 e mostra as 8 tabelas com 0 linhas.

## 7. Dados do seed (`Data/Seed/*.json`)

- [x] 7.1 Criar `tools/seed-generator/gerar_seed.py`, versionado, que recebe a pasta do protótipo (`C:\Users\kendj\wrk\world-cup\prototipo`) e a pasta de saída (`src/PortalCopa26/PortalCopa26/Data/Seed/`). O script:
  - lê `fontes/*.txt` e os códigos FIFA e `copas` de `js/site.js`;
  - aplica os mapas de aliases de seleção e de cidade do design D8;
  - lê `copa2026_grupos.txt` apenas nas linhas A–L, tolerando o TAB dentro de "Estados Unidos";
  - falha ao encontrar seleção ou cidade sem mapeamento, ou grupo com número de seleções diferente de 4.

  Verificar que ele roda até o fim sem erros de mapeamento.
- [x] 7.2 Gerar `grupos.json` (12) e `selecoes.json` (48, com código, nome, técnico, grupo e cabeça de chave = primeira coluna de `copa2026_grupos.txt`); verificar as contagens, 12 cabeças de chave, grupo C = BRA (cabeça), MAR, HAI, SCO, grupo D contendo USA, e técnico do Brasil = Carlo Ancelotti.
- [x] 7.3 Gerar `estadios.json` (16, capacidade numérica, Toronto = 30000); verificar a contagem e o Estádio Azteca / Cidade do México / México / 87523.
- [x] 7.4 Gerar `jogos.json` com os 104 jogos:
  - 1–72: fase de grupos;
  - 73–88: Segunda Fase, com as seleções definidas;
  - 89–104: mata-mata, com descrições de vaga.

  Cada jogo resolve seu estádio pela cidade, via mapa de aliases, e tem o horário de Brasília convertido para UTC, incluindo os jogos que viram o dia. Verificar:
  - 104 jogos, 6 por grupo, e todos os 16 estádios usados;
  - jogo 1 = MEX x RSA em 2026-06-11T19:00Z;
  - jogo em Santa Clara → Levi's Stadium;
  - "Segundafase 9" = BRA x JPN em Houston;
  - final = "Venc. Semifinal 1" x "Venc. Semifinal 2" em 19/07, no MetLife Stadium.
- [x] 7.5 Gerar `jogadores.json` (cerca de 1.238, por código FIFA, posição mapeada para o enum e `ParticipacoesCopas` vindo de `site.js` por nome normalizado, senão 0); verificar que as 48 seleções têm elenco e que existe Lionel Messi / ARG / Atacante / 38 / 20 gols.
- [x] 7.6 Gerar `ranking.json` (98 entradas, com código FIFA apenas para participantes; "EA Unidos" sem vínculo); verificar que a posição 1 = Argentina 1877.72 → ARG, que a Itália está na 12ª posição sem vínculo e que os 7 participantes ausentes (CUW, CPV, UZB, NZL, GHA, BIH, KSA) não aparecem.
- [x] 7.7 Marcar `Data/Seed/*.json` como `CopyToOutputDirectory=PreserveNewest` no `.csproj`; verificar que os arquivos aparecem em `bin/.../Data/Seed/` após o build.

## 8. Seeder

- [x] 8.1 Implementar `DatabaseSeeder` (design D8) e chamá-lo no `DatabaseInitializer` logo após `MigrateAsync`. O seeder:
  - lê os JSON e resolve as referências por código FIFA, código de grupo ou cidade;
  - carrega cada tabela somente se estiver vazia, na ordem Grupos → Seleções → Estádios → Jogos → Jogadores → Ranking, numa única transação;
  - registra as contagens no log;
  - não acessa `Simulacao` nem `SimulacaoJogo`.

  Verificar com o banco apagado: o log e `verificar_banco.py contagens` mostram 12/48/16/104/~1238/98.
- [x] 8.2 Verificar a idempotência: reiniciar a aplicação e confirmar que `verificar_banco.py contagens` devolve as mesmas contagens e que o log indica que o seed já foi aplicado.

## 9. Assets de front-end

- [x] 9.1 Vendorizar `chart.umd.min.js` (Chart.js 4.x, versão exata registrada num comentário ou README em `wwwroot/lib/chartjs/`) e referenciá-lo em `Components/App.razor`; verificar que `curl -k https://localhost:<porta>/lib/chartjs/chart.umd.min.js` retorna 200 e, no navegador, que `typeof Chart === "function"`.
- [x] 9.2 Confirmar que o Bootstrap 5 do template é servido de `wwwroot/lib/bootstrap/` (sem CDN); verificar pelo HTML de `GET /`.

## 10. Verificação integrada

- [x] 10.1 Persistência de simulação: com a aplicação parada, rodar `verificar_banco.py simulacao-exemplo`; depois iniciar e encerrar a aplicação e verificar com `contagens` que a `Simulacao` e seus dois `SimulacaoJogo` continuam intactos e que o seed não os alterou.
- [x] 10.2 Restrições: rodar `verificar_banco.py restricoes`; verificar que ele reporta falha na duplicidade de (SimulacaoId, JogoId), remoção em cascata dos `SimulacaoJogo` ao excluir a `Simulacao` e rejeição da exclusão de um `Jogo` referenciado.
- [x] 10.3 Rodar `dotnet build` da solução sem warnings novos relevantes e `openspec validate portal-copa26 --strict`; verificar que ambos passam.

## Workflow follow-up

- Arquivar o change (`/openspec-archive-change`) após a revisão da implementação.
- Próximos changes sugeridos: Landing Page (layout e navegação do protótipo + gráfico do Ranking via Chart.js/JSInterop), Jogos, Grupos (RN-01), Seleções, Simulador (RN-01/RN-02 e classificações persistidas).
