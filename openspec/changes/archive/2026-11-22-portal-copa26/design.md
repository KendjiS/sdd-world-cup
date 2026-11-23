# Design

## Context

Veja `proposal.md` (Why). Estado atual e restrições relevantes:

- `src/` está vazio. Não há código a migrar.
- `CLAUDE.md` exige .NET 10, Blazor Web App, EF Core, SQLite, Bootstrap 5, Chart.js via JSInterop, projeto único e solução `.slnx`, com páginas em `Components/Pages`.
- **Pré-requisito de ambiente:** hoje a máquina tem apenas o SDK .NET 11.0.100. O runtime instalado é `Microsoft.AspNetCore.App` 11.0.0; o `Microsoft.NETCore.App` 10.0.11 existe, mas sem o ASP.NET Core 10. O template `blazor` do SDK 11 só aceita `-f net11.0`. Por isso o **SDK .NET 10** (que inclui o runtime ASP.NET Core 10) precisa ser instalado antes da implementação, e a solução fixa esse SDK via `global.json` (D2).
- O Python 3.11 está disponível e o `sqlite3` CLI não. Os scripts de apoio (geração do seed e verificação do banco) usam Python com o módulo `sqlite3` da biblioteca padrão.
- As fontes de dados estão em `C:\Users\kendj\wrk\world-cup\prototipo\fontes\*.txt`, em texto semiestruturado e com nomes de seleções inconsistentes entre arquivos. Exemplos: Holanda/Países Baixos, Coreia do Sul/República da Coreia, Tchéquia/República Tcheca, "Bósnia", "RD Congo". Atenção: "EA Unidos" no ranking são os Emirados Árabes, não os EUA.
- Os códigos FIFA de três letras (`GRUPOS`, `SELECOES`) e o campo `copas` dos jogadores existem apenas em `prototipo/js/site.js`. O restante desse arquivo está desatualizado e não é fonte de verdade.
- Os horários das fontes estão em horário de Brasília (UTC−3). Alguns jogos viram o dia, por exemplo "01:00 hs (14 de junho)".
- Os nomes de estádio nos jogos são neutros, no padrão FIFA (ex.: "Estádio de Boston"). Os nomes comerciais estão em `copa2026_cidades_sede_estadios.txt`, e a correspondência se faz pela cidade.

## Goals / Non-Goals

**Goals:**
- Base executável que as próximas mudanças (páginas, ranking, simulador) usam sem reestruturação.
- Fronteiras de pasta que já correspondam às futuras camadas Domain / Application / Infrastructure / Web.
- Seed reproduzível, revisável em code review e independente de rede.

**Non-Goals:**
- Serviços de consulta por página (ex.: `IJogoService.ObterPorData`). Eles nascem com cada página, guiados pela necessidade real.
- Componente de gráfico e módulo JS de interop do Chart.js. Ficam para o change do Ranking.
- Cálculo de classificação (RN-01/RN-02) e persistência de classificações simuladas. Ficam para o change do Simulador.
- Layout e navegação do protótipo. Fica para o change da Landing Page.
- Projeto de testes. A estrutura exigida prevê um único projeto, e a verificação desta mudança é feita executando a aplicação.

## Decisions

### D1. Template e modo de renderização
`dotnet new blazor -n PortalCopa26 -f net10.0 --interactivity Server --all-interactive false`, executado em `src/PortalCopa26/` depois que o `global.json` (D2) já seleciona o SDK 10. Com o SDK 11 ativo, o template rejeita `net10.0`. A renderização é estática por padrão, com Interactive Server ativado por página ou componente.
- *Por quê:* Interactive Server acessa EF Core diretamente, sem API intermediária, e isso basta para um portal de consulta com simulador local.
- *Alternativas:* WebAssembly/Auto exigiria uma API HTTP e um projeto `.Client`, o que contraria o requisito de projeto único. A interatividade global prenderia toda página a um circuito SignalR sem necessidade.

### D2. Solução `.slnx`
1. `dotnet new globaljson --sdk-version <10.0.x instalado> --roll-forward latestFeature` em `src/PortalCopa26/`.
2. `dotnet new sln -n PortalCopa26 --format slnx`.
3. `dotnet sln add PortalCopa26/PortalCopa26.csproj`.
- *Por que `global.json`:* garante que template, build e `dotnet ef` usem o SDK 10 mesmo com o SDK 11 instalado. `latestFeature` aceita patches e feature bands mais novos do 10.0, mas nunca o 11.
- *Alternativas rejeitadas:* gerar com `net11.0` e rebaixar o TFM à mão (o template de .NET 11 pode trazer APIs ausentes no 10, e o app não teria runtime ASP.NET Core 10 para rodar); `RollForward=Major` (executaria no runtime 11, ou seja, não seria de fato .NET 10).

### D3. Estrutura de pastas e namespaces

```
PortalCopa26/
├── Components/        Layout/, Pages/, Shared/ (template + futuros componentes)
├── Models/            Entidades e enums (POCOs puros)       → futuro projeto Domain
├── Services/          Interfaces + implementações           → futuro projeto Application
├── Data/
│   ├── PortalCopaDbContext.cs
│   ├── Configurations/  IEntityTypeConfiguration<T> por entidade
│   ├── Migrations/
│   └── Seed/            DatabaseInitializer, DatabaseSeeder, *.json
├── Extensions/        ServiceCollectionExtensions, WebApplicationExtensions
├── App_Data/          portalcopa26.db (gitignored)
└── wwwroot/lib/       bootstrap/ (template), chartjs/
```

Fora de `src/`, na raiz do workspace, ficam as ferramentas de apoio. Elas não são código da aplicação e não entram na solução:

```
tools/
├── seed-generator/   gerar_seed.py: .txt do protótipo → Data/Seed/*.json (D8)
└── db-check/         verificar_banco.py: contagens e restrições do banco SQLite
```

- Os namespaces seguem as pastas (`PortalCopa26.Models`, `PortalCopa26.Data`…), para que mover uma pasta para outro projeto mude só referências.
- As páginas ficam em `Components/Pages`, convenção do template do .NET 10, agora também registrada no `CLAUDE.md`.
- Os mapeamentos usam somente Fluent API em `Data/Configurations`, sem Data Annotations nas entidades. Assim `Models/` fica livre de EF, como exige o spec `app-foundation`.

### D4. Modelo de entidades

| Entidade | Campos principais | Relacionamentos / restrições |
|---|---|---|
| `Grupo` | Id, Codigo (`char`/string(1)) | Codigo único; 1–N Selecao e 1–N Jogo, ambos com `Restrict` |
| `Selecao` | Id, CodigoFifa (string(3)), Nome, Tecnico, GrupoId, CabecaDeChave | CodigoFifa único; `BandeiraUrl` calculada (não mapeada) |
| `Jogador` | Id, SelecaoId, Nome, Posicao (enum), Idade, Gols, ParticipacoesCopas | N–1 Selecao (cascade) |
| `Estadio` | Id, Nome, Cidade, Pais (enum `PaisSede`), Capacidade | 1–N Jogo |
| `Jogo` | Id, Numero (1..104), Fase (enum), Rotulo?, GrupoId?, EstadioId, DataHoraUtc, SelecaoMandanteId?, SelecaoVisitanteId?, DescricaoMandante?, DescricaoVisitante?, GolsMandante?, GolsVisitante? | Numero único; FKs de seleção com `Restrict` |
| `RankingFifa` | Id, Posicao, NomeSelecao, Pontos (`decimal(7,2)`), SelecaoId? | Posicao única; índice único simples em SelecaoId (o SQLite já aceita vários NULL num índice único, então não precisa de filtro) |
| `Simulacao` | Id, Nome, CriadaEmUtc, AtualizadaEmUtc | 1–N SimulacaoJogo (cascade) |
| `SimulacaoJogo` | Id, SimulacaoId, JogoId, GolsMandante, GolsVisitante | índice único (SimulacaoId, JogoId); FK Jogo `Restrict` |

Enums: `Posicao { Goleiro, Defensor, MeioCampista, Atacante }`, `Fase { Grupos, SegundaFase, Oitavas, Quartas, Semifinal, TerceiroLugar, Final }`, `PaisSede { Canada, EstadosUnidos, Mexico }`. Todos são persistidos como string (`HasConversion<string>()`), para legibilidade do banco e para que reordenar valores não corrompa dados.

- *Mata-mata:* os FKs de seleção são opcionais e acompanhados de descrições de vaga (`"Venc. Segundafase 1"`). Os confrontos ficam representáveis já, antes da lógica de chaveamento, e a lógica futura só preenche os FKs. A alternativa seria uma tabela de "vagas" com referência ao jogo de origem. Ela foi adiada porque é mais rica, mas só se justifica quando o simulador propagar vencedores.
- *Ranking sem FK obrigatório:* a fonte contém 98 seleções, entre elas não classificadas (Itália, Nigéria…). Sete participantes não aparecem na fonte (Curaçao, Cabo Verde, Uzbequistão, Nova Zelândia, Gana, Bósnia e Herzegovina, Arábia Saudita) e ficam sem entrada.
- *`BandeiraUrl` calculada:* o padrão de URL da FIFA é derivado do código, e não há o que persistir.

### D5. Datas
Os horários são armazenados como `DateTime` em UTC (`DataHoraUtc`). O seed converte de Brasília com o offset fixo −03:00, já que o Brasil não tem horário de verão desde 2019. A exibição futura converte para o fuso desejado.
- *Por quê:* o provider SQLite do EF Core não traduz `ORDER BY` em `DateTimeOffset`, e o spec exige ordenação por data. Um `DateTime` UTC (texto ISO) ordena corretamente.

### D6. DbContext e DI
- `builder.Services.AddPortalCopaData(builder.Configuration, builder.Environment)` chama `AddDbContextFactory<PortalCopaDbContext>(o => o.UseSqlite(cs))`.
- *Por quê:* em Blazor Server o escopo de DI dura o circuito inteiro. Um `DbContext` com escopo seria compartilhado entre operações concorrentes, e a factory com um contexto curto por operação é a recomendação oficial.
- `AddPortalCopaServices()` é criado agora, ainda vazio exceto pelo `DatabaseInitializer`, como ponto único onde os próximos changes registram seus serviços.
- A connection string em `appsettings.json` é `"PortalCopa": "Data Source=App_Data/portalcopa26.db"`.
- **Caminho do banco:** um `Data Source` relativo é resolvido pelo SQLite contra o diretório de execução atual, não contra o projeto. Rodar `dotnet run --project ...` de outra pasta criaria um segundo banco vazio, e as simulações pareceriam perdidas. Por isso `AddPortalCopaData` lê a connection string com `SqliteConnectionStringBuilder` e, se o `DataSource` for relativo, o combina com `IWebHostEnvironment.ContentRootPath`. Caminhos absolutos são respeitados. O diretório do arquivo é criado na inicialização, se não existir.
- O caminho já resolvido é registrado no log no startup, para facilitar o diagnóstico.

### D7. Inicialização do banco
`await app.InitializeDatabaseAsync()` (extensão em `WebApplication`) roda antes de `app.Run()`:
1. `Database.MigrateAsync()`, para criar ou atualizar o schema.
2. `DatabaseSeeder.SeedAsync()`, para a carga de dados.
- *Alternativa rejeitada:* `EnsureCreated`, que é incompatível com migrations futuras e quebraria a evolução do schema sem perder simulações.
- `dotnet-ef` é instalado como ferramenta local (`dotnet new tool-manifest` + `dotnet tool install dotnet-ef --version 10.0.x`), para builds reproduzíveis.
- **Versões fixas:** `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design` e `dotnet-ef` usam a mesma versão exata (10.0.15, a mais recente publicada no momento do apply). Ela fica registrada no `.csproj` e em `src/PortalCopa26/dotnet-tools.json` (o SDK 10 cria o manifesto na pasta atual, não em `.config/`), sem curingas (`10.*`), para que builds em momentos diferentes resolvam os mesmos pacotes.

### D8. Seed: JSON normalizado + seeder idempotente
1. **Arquivos de dados.** `Data/Seed/*.json` (`grupos`, `selecoes`, `estadios`, `jogos`, `jogadores`, `ranking`) são gerados uma vez a partir dos `.txt` e versionados. São copiados para a saída (`CopyToOutputDirectory=PreserveNewest`). Todas as referências entre arquivos usam o **código FIFA** e o **código do grupo**, nunca nomes livres.
2. **Normalização na geração.** Um mapa de aliases leva cada grafia das fontes ao código FIFA:

   | Grafia | Código |
   |---|---|
   | Holanda, Países Baixos | NED |
   | Coreia do Sul, República da Coreia | KOR |
   | Tchéquia, República Tcheca | CZE |
   | EUA, Estados Unidos | USA |
   | Bósnia, Bósnia e Herzegovina | BIH |
   | RD Congo, ReD do Congo, República Democrática do Congo | COD |
   | Costa do Marfim, Côte d'Ivoire | CIV |
   | Irã, República Islâmica do Irã | IRN |

   O nome exibido é a forma usada em `copa2026_grupos.txt`.

   Um segundo mapa leva cada grafia de **cidade** das fontes de jogos à cidade do estádio em `copa2026_cidades_sede_estadios.txt`:

   | Grafia nas fontes de jogos | Cidade do estádio |
   |---|---|
   | Santa Clara | San Francisco (Levi's Stadium) |
   | Nova Iorque, Nova Iorque/Nova Jersey, Nova Jersey, Nova York/Nova Jersey | Nova York/New Jersey (MetLife Stadium) |
   | Azteca, Cidade do México | Cidade do México (Estádio Azteca) |
   | Seattle Field | Seattle |
   | Vancouver Place | Vancouver |

   As demais cidades coincidem com o nome da tabela de estádios. O script falha se uma cidade não for mapeada para exatamente um dos 16 estádios.

   **Leitura de `copa2026_grupos.txt`:** o arquivo é separado por TAB, mas não de forma uniforme. Na linha do grupo D, "Estados Unidos" está dividido por um TAB (`D\tEstados\tUnidos\tParaguai…`), e as colunas usam TABs repetidos para alinhamento. Depois do grupo L há um trecho de PRD colado ("## 3. Requisitos Funcionais…"). O script deve:
   - ler apenas as linhas que começam com uma letra de A a L seguida de TAB, e parar após o grupo L;
   - descartar campos vazios;
   - casar nomes compostos contra o mapa de aliases, tentando a junção de tokens consecutivos ("Estados" + "Unidos");
   - exigir exatamente 4 seleções por grupo e 48 no total, e falhar caso contrário.
3. **Origem por arquivo.**

   | Dado | Fonte |
   |---|---|
   | Grupos, seleções, cabeças de chave | `copa2026_grupos.txt`: primeira coluna = cabeça de chave, o que cobre os 12 grupos (`cabecas-chave.txt` lista só 11) |
   | Códigos FIFA | `GRUPOS` em `site.js` |
   | Técnicos | `copa2026_pais_tecnicos.txt` |
   | Estádios | `copa2026_cidades_sede_estadios.txt`; Toronto `30.000*` → 30000 |
   | Jogos 1–72 | `copa2026_jogos_primeira_fase.txt`, na ordem cronológica do arquivo |
   | Jogos 73–88 | `copa2026_Jogos_Segunda_fase.txt`, com `Rotulo` = "Segundafase N" |
   | Jogos 89–104 | Oitavas, quartas, semifinais, 3º lugar e final |
   | Jogadores | `copa2026_selecoes_jogadores.txt` (`Nome\|Idade\|Posição\|Gols`); `ParticipacoesCopas` vem de `site.js` quando o nome bate exatamente (após normalizar acentos e caixa), senão 0 |
   | Ranking | `copa2026_ranking_fifa.txt` (98 linhas), com vínculo pela tabela de aliases |

4. **Geração.** Os JSON são produzidos pelo script Python versionado `tools/seed-generator/gerar_seed.py`, que recebe como argumentos a pasta do protótipo e a pasta de saída (`src/PortalCopa26/PortalCopa26/Data/Seed/`). Ele fica fora de `src/` porque não é código da aplicação, mas é versionado: quando uma fonte for corrigida, basta rodar de novo para regenerar os JSON de forma reproduzível. Ao final, o script confere as contagens do spec `seed-data` e falha se alguma divergir. Gerar em tempo de execução a partir dos `.txt` foi rejeitado: parsing frágil de texto livre a cada início e dependência de arquivos externos ao projeto.
5. **Execução.** O `DatabaseInitializer` chama o `DatabaseSeeder` depois das migrations. O `DatabaseSeeder` carrega por tabela, em ordem de dependência (Grupos → Seleções → Estádios → Jogos → Jogadores → Ranking). Cada tabela só é carregada se estiver vazia (`AnyAsync`), tudo numa transação única, e o seeder registra as contagens no log. Ele nunca toca `Simulacao` e `SimulacaoJogo`.
6. **Alternativa rejeitada:** `HasData` nas configurações. Com cerca de 1.400 linhas, inflaria a migration inicial, exigiria IDs fixos manuais e tornaria cada correção de dado uma nova migration.

### D9. Chart.js
O arquivo `chart.umd.min.js` (Chart.js 4.x) é vendorizado em `wwwroot/lib/chartjs/` e referenciado em `Components/App.razor` após o script do Blazor. Nenhum componente é criado nesta mudança. O change do Ranking adicionará `wwwroot/js/charts.js` (módulo ES via `IJSRuntime.InvokeAsync<IJSObjectReference>("import", …)`) e um componente `BarChart` reutilizável.
- *Por quê local e não CDN:* funciona offline e evita dependência externa, em linha com o espírito de "sem integrações externas".

## Risks / Trade-offs

- **[Grafias divergentes nas fontes]** Um alias de seleção ou de cidade esquecido pode gerar seleção órfã, jogo sem time ou jogo sem estádio. → O script de geração falha ao encontrar nome ou cidade sem mapeamento, e as contagens do spec (48/72/16/104) são conferidas antes de versionar os JSON.
- **[Formato irregular de `copa2026_grupos.txt`]** O TAB dentro de "Estados Unidos" e o texto colado após o grupo L podem gerar seleções a mais ou a menos. → Leitura restrita às linhas A–L e validação de 4 seleções por grupo (D8).
- **[Sem `sqlite3` CLI]** As verificações das tarefas não podem depender dele. → O script `tools/db-check/verificar_banco.py` usa o módulo `sqlite3` do Python para conferir contagens, persistência e restrições.
- **[Correspondência de nomes para `copas`]** Os nomes em `site.js` e nos `.txt` diferem, e muitos jogadores ficarão com 0. → Isso é aceito e registrado no spec. Uma correção futura de dados exige apenas atualizar `jogadores.json`.
- **[Ranking incompleto]** Sete participantes não têm entrada no ranking. → Fica sem valor, sem inventar. As telas futuras devem exibir "—" e o desempate por ranking (RN-01 item 7) deve tratá-las como último. Isso será decidido no change do Simulador.
- **[Seed "por tabela vazia"]** Se uma tabela for parcialmente apagada à mão, ela não é recarregada. Se os JSON forem corrigidos depois, o banco existente não é atualizado. → É aceitável para esta fase sem área administrativa. Para reaplicar, apaga-se `App_Data/portalcopa26.db`, mas isso perde as simulações. Uma estratégia de upsert por chave natural pode vir depois.
- **[SDK .NET 10 ausente]** Sem ele, o template rejeita `net10.0` e o app não tem o runtime ASP.NET Core 10. → Pré-requisito explícito (tarefa 0.1): a implementação não começa até `dotnet --list-sdks` mostrar um 10.0.x e `dotnet --list-runtimes` mostrar `Microsoft.AspNetCore.App 10.0.x`. O `global.json` impede que o SDK 11 seja usado por engano.
- **[Datas com offset fixo]** Um offset fixo de −3h está correto para Brasília em 2026, mas não vale para outros fusos. → A conversão é isolada no seed, e o banco guarda UTC.

## Migration Plan

É uma mudança greenfield, sem dados anteriores. Rollback = remover `src/PortalCopa26` (o banco fica dentro dele, em `PortalCopa26/App_Data/`) e `tools/`. A partir daqui, toda alteração de schema entra como nova migration aplicada automaticamente na inicialização (D7), preservando simulações.

## Open Questions

- Fuso de exibição dos horários (Brasília fixo ou fuso do navegador). Será decidido no change da página de Jogos e não afeta o armazenamento em UTC.
