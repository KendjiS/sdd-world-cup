# Design

## Context

Estado atual do projeto `src/PortalCopa26/PortalCopa26/`:

- `Program.cs` registra Razor Components com `AddInteractiveServerComponents()`, mas não há render mode global: as páginas são renderizadas no servidor sem interatividade (static SSR), a menos que declarem um render mode.
- A persistência é registrada com `AddDbContextFactory<PortalCopaDbContext>` (SQLite). `Services/` está vazio (só `.gitkeep`). Os serviços de aplicação são registrados em `AddPortalCopaServices()` (`Extensions/ServiceCollectionExtensions.cs`).
- `App.razor` já carrega `lib/bootstrap/dist/css/bootstrap.min.css`, `app.css` e `lib/chartjs/chart.umd.min.js` localmente.
- Layout e páginas ainda são os do template: `MainLayout` com sidebar, `NavMenu`, `Home` ("Hello, world!"), `Counter` e `Weather`.
- Dados relevantes: `Jogo.DataHoraUtc` (UTC, convertido de Brasília UTC−3 no seed), `Jogo.GrupoId?`, `SelecaoMandante/Visitante?` com `DescricaoMandante/Visitante` para vagas indefinidas, `Estadio.Pais` (`PaisSede`), `RankingFifa` (98 entradas, `NomeSelecao`, `Pontos`), `Selecao.BandeiraUrl` (não persistida).
- O protótipo (`..\prototipo\index.html` e `css/site.css`) usa nomes de classe que colidem com o Bootstrap (`.btn`, `.card`, `.badge`, `.nav`, `.page`, utilitários `.mb-2`, `.text-muted` etc.) e um reset global (`* { margin:0; padding:0 }`).

Requisitos: ver `specs/landing-page/spec.md` e `specs/portal-layout/spec.md`.

## Goals / Non-Goals

**Goals:**
- Página inicial em static SSR, com interatividade apenas onde é necessária (o gráfico).
- Integração com Chart.js genérica, reutilizável para qualquer tipo de gráfico futuro.
- Serviços de consulta reutilizáveis pelas próximas páginas (Jogos, Ranking), sem acoplamento à Landing Page.
- Visual fiel ao protótipo, sem quebrar o Bootstrap 5.

**Non-Goals:**
- Testes automatizados de UI e criação de um projeto de testes. A verificação deste change é por build e execução manual (seção Migration Plan).
- Cache dos dados da Landing Page.
- Tradução ou internacionalização (a UI é em pt-BR).

## Decisions

### 1. Static SSR na página, interatividade só no gráfico
`Home.razor` (`@page "/"`) e os componentes `HeroSection`, `EstatisticasCopa`, `ProximosJogos` e `SimuladorPainel` são renderizados em static SSR. Apenas `RankingFifaChart` é usado com `@rendermode="InteractiveServer"`, porque o Chart.js precisa de `OnAfterRenderAsync`, que não executa em static SSR.

- *Por quê:* não abre um circuito SignalR para conteúdo estático. O HTML chega completo na primeira resposta, inclusive para o requisito de link ativo e do menu.
- *Alternativa descartada:* render mode global `InteractiveServer`. É mais simples, mas mantém um circuito por visitante sem necessidade.

### 2. Home orquestra os dados, componentes são apresentacionais
`Home.razor` injeta os serviços, carrega os dados em `OnInitializedAsync` em paralelo (`Task.WhenAll`) e repassa cada fatia por `[Parameter]` aos componentes. Os componentes de `Components/Pages/LandingPage/` não injetam serviços nem `DbContext`.

- *Por quê:* os componentes ficam reutilizáveis (podem receber outros dados, por exemplo jogos de uma seleção) e testáveis sem banco. Cada serviço cria o próprio `DbContext` pela factory, o que torna seguras as consultas paralelas.
- `RankingFifaChart` recebe os itens do ranking por parâmetro. Como é um componente interativo instanciado a partir de static SSR, o parâmetro é serializado no marcador de prerender. Por isso os DTOs são `record`s simples e serializáveis (sem entidades EF), e o banco é consultado uma única vez.
- *Alternativa descartada:* cada componente injetar seu serviço. Isso acopla o componente à fonte de dados e, no componente interativo com prerender, faria a consulta duas vezes.

### 3. Serviços de consulta por assunto, não por página
Em `Services/`, com interfaces e implementações:

| Serviço | Método | Retorno |
|---|---|---|
| `ITorneioService` | `ObterResumoAsync()` | `ResumoTorneioDto` (totais de seleções, grupos, jogos e estádios) |
|  | `ObterPaisesSedeAsync()` | `IReadOnlyList<PaisSedeDto>` (nome, código FIFA, quantidade de estádios), na ordem EUA, Canadá, México |
| `IJogoService` | `ObterProximosJogosAsync(DateOnly dataReferencia, int quantidadeDias)` | `IReadOnlyList<DiaJogosDto>` (data e lista de `JogoResumoDto`) |
| `IRankingFifaService` | `ObterTopAsync(int quantidade)` | `IReadOnlyList<RankingItemDto>` (posição, seleção, código FIFA?, pontos) |

- DTOs em `Models/Dtos/`, como `record`s sem dependência de EF ou UI (mantém a regra de `app-foundation`).
- Consultas com `AsNoTracking()` e projeção (`Select`) direto para DTO.
- Registro em `AddPortalCopaServices()` como `Scoped`.
- *Por quê:* as páginas Jogos e Ranking dos próximos changes reutilizam `IJogoService` e `IRankingFifaService`. Um `ILandingPageService` único concentraria responsabilidades de várias áreas (fere o SRP) e não seria reaproveitado.

### 4. Próximos jogos: data de referência e fuso de Brasília
- Configuração `PortalCopa:ProximosJogos:DataReferencia` (formato `yyyy-MM-dd`) em `appsettings.json`, com valor `2026-06-05` (o mesmo do protótipo). Lida por `IOptions<ProximosJogosOptions>`.
- `ProximosJogosOptions.DataReferencia` é `string?`, e não `DateOnly?`. O binder de configuração lança exceção ao ler um `DateOnly` inválido, mas o spec exige que uma data inválida caia na data atual. A conversão usa `DateOnly.TryParseExact(valor, "yyyy-MM-dd", CultureInfo.InvariantCulture, ...)`.
- Quando a data está ausente ou é inválida, usa-se a data atual em Brasília, obtida via `TimeProvider` (injetado e registrado com `TryAddSingleton(TimeProvider.System)`) para permitir testes futuros.
- Fuso fixo de Brasília (UTC−3, sem horário de verão desde 2019), o mesmo usado na conversão do seed. Fica centralizado em um helper `HorarioBrasilia` (`ParaBrasilia(DateTime utc)` e `InicioDoDiaUtc(DateOnly)`) em `Services/`.
- O provider SQLite devolve `DataHoraUtc` com `DateTimeKind.Unspecified`. Por isso `ParaBrasilia` trata o valor recebido como UTC independentemente do `Kind`: subtrai 3 horas e nunca chama `ToLocalTime()` nem `ToUniversalTime()`, que dependeriam do fuso do servidor.
- Algoritmo: converte o início do dia de referência para UTC; busca os jogos com `DataHoraUtc >= limite`, ordenados por `DataHoraUtc`, com includes de seleções, grupo e estádio projetados em DTO; agrupa em memória pela data em Brasília e pega os `quantidadeDias` (2) primeiros grupos. O volume é pequeno (no máximo 104 jogos), então o agrupamento em memória é aceitável. A ordenação no banco funciona porque `DataHoraUtc` é `DateTime` (decisão já tomada na fundação).
- *Alternativa descartada:* `TimeZoneInfo` "America/Sao_Paulo". Dá o mesmo resultado em 2026, mas depende dos IDs de fuso do sistema operacional (Windows e IANA). O offset fixo é coerente com o seed.

### 5. Componente Chart.js genérico via módulo JS
- `wwwroot/js/charts.js`: um **módulo ES** que exporta `criarGrafico(canvas, config)`, `atualizarGrafico(canvas, config)` e `destruirGrafico(canvas)`. As instâncias ficam em um `WeakMap` por canvas, e o módulo usa o `Chart` global já carregado pelo `App.razor`.
- `Components/Shared/Charts/ChartJs.razor`: componente genérico com parâmetros `Configuracao` (objeto serializável com a configuração do Chart.js: `type`, `data`, `options`), `Altura` e `CssClass`. Ele renderiza um `<canvas>` com `@ref`. No primeiro `OnAfterRenderAsync`, importa o módulo (`IJSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/charts.js")`) e cria o gráfico. Quando os parâmetros mudam, atualiza. Implementa `IAsyncDisposable` para destruir o gráfico e liberar o módulo, tratando `JSDisconnectedException`.
- Callbacks JS (como o formatador da dica e dos ticks) não são serializáveis. Para resolver isso, o módulo aceita a opção declarativa `formatoNumero: "pt-BR"` e, se ela estiver presente, injeta os callbacks `toLocaleString('pt-BR')` na dica e nos ticks do eixo de valores antes de criar o gráfico. Outras convenções declarativas podem ser acrescentadas no módulo sem mudar o componente.
- `RankingFifaChart.razor` (em `LandingPage/`) monta a configuração de barras horizontais (`indexAxis: 'y'`), as cores do pódio (`#f5a623`, `#adb5c7`, `#cd7f32`) e o azul (`rgba(74,136,220,0.75)` com borda `#4a88dc`), além do estilo da dica e das grades do protótipo. O protótipo fixa `x.min = 1600` para realçar as diferenças entre as barras. Aqui o mínimo é calculado a partir dos dados: a menor pontuação menos 100, arredondada para baixo na centena. Com os dados atuais, o resultado é o mesmo 1600, e uma atualização do ranking com pontuações menores não faz barras sumirem. Depois delega a renderização ao `ChartJs`.
- *Por quê:* o JSInterop fica isolado em um único componente e um único módulo, conforme a regra do CLAUDE.md de usar JSInterop só quando necessário. Futuros gráficos (estatísticas de grupos e jogadores) só montam a configuração.
- *Alternativas descartadas:* script global com `window.xxx`, que polui o escopo global e não isola por módulo; e uma biblioteca wrapper de terceiros (ChartJs.Blazor e similares), que adiciona dependência e muitas vezes fica defasada em relação ao Chart.js 4.

### 6. Tema: Bootstrap 5 com variáveis e CSS do protótipo prefixado
- `wwwroot/css/portal.css`, carregado após o Bootstrap no `App.razor`, contém:
  - Tokens do protótipo (`--pc-bg`, `--pc-card`, `--pc-gold` etc.) e o mapeamento para as variáveis do Bootstrap (`--bs-body-bg`, `--bs-body-color`, `--bs-border-color`, `--bs-card-bg`…), com `data-bs-theme="dark"` no `<html>`. O mapeamento é declarado em `:root, [data-bs-theme=dark]`, para sobrescrever as variáveis do tema escuro do Bootstrap independentemente da especificidade.
  - Variantes que o Bootstrap não tem: `.btn-gold`, `.btn-outline-portal`, `.badge-next` e `.badge-group`.
- O reset global do protótipo **não** é portado (o Bootstrap já faz o reboot). Os utilitários do protótipo (`.mb-2`, `.flex`, `.fw-7`…) são substituídos pelos equivalentes do Bootstrap.
- Os blocos específicos (hero, host-card, stat-card, game-card, day-group) ficam em **CSS isolado** de cada componente (`.razor.css`), copiados do protótipo e com as cores trocadas pelos tokens. O CSS isolado não alcança elementos de componentes filhos. Por isso os tamanhos de bandeira (`xs`/`sm`/`md`/`lg`) ficam no `.razor.css` do próprio `BandeiraSelecao`, e um pai só usa `::deep` quando precisa ajustar o filho. A grade usa `row`/`col` do Bootstrap. Por exemplo, Ranking e Simulador usam `col-lg-8` e `col-lg-4`, o que empilha abaixo de 992 px, conforme o spec.
- Componentes Bootstrap (`card`, `btn`, `badge`) são usados com as classes do Bootstrap e ganham a aparência do protótipo pelas variáveis.
- *Por quê:* atende ao requisito de usar Bootstrap 5 sem conflito de nomes e mantém o CSS de cada bloco junto do seu componente.
- *Alternativa descartada:* copiar `site.css` inteiro como global. As colisões em `.btn`, `.card`, `.nav` e `.badge` quebrariam o Bootstrap.

### 7. Layout e imagens da FIFA
- `MainLayout.razor` é reescrito com `PortalNavbar` (marca, logo e `NavLink`s com `Match="NavLinkMatch.All"` no Home; o destaque ativo funciona em SSR), `<main>` com `@Body` e `PortalFooter`. Os dois ficam em `Components/Layout/`.
- Menu móvel apenas com CSS (padrão checkbox + label, como o template já usa), sem interatividade do servidor, para atender ao spec de funcionar sem componente interativo.
- A navegação aprimorada (enhanced navigation) do Blazor atualiza o DOM sem recarregar a página, e com isso o checkbox continuaria marcado: o menu ficaria aberto após clicar em um link. Para evitar isso, `App.razor` inclui um script inline de poucas linhas, depois de `blazor.web.js`, que desmarca o checkbox no evento `Blazor.addEventListener('enhancedload', ...)`. Não é JSInterop, apenas um listener de página.
- `Components/Shared/BandeiraSelecao.razor`: recebe `CodigoFifa`, `Tamanho` (`xs`/`sm`/`md`/`lg`) e `Nome`. Renderiza o `<img>` e um `<span>` oculto com o código. O `onerror` em HTML puro (atributo string, não evento Blazor) oculta a imagem e exibe o código, como no `flagImg` do protótipo. O logo usa `onerror="this.style.display='none'"`.
- Os países-sede (nome de exibição e código FIFA: USA, CAN, MEX) são mapeados a partir do enum `PaisSede` em `ITorneioService`, que conta os estádios por `Estadio.Pais`.
- `App.razor`: `lang="pt-BR"`, `data-bs-theme="dark"`, link para `css/portal.css` e título padrão. `Home` define `<PageTitle>PortalCopa26 — Copa do Mundo FIFA 2026</PageTitle>`.
- Remoção de `Counter.razor`, `Weather.razor`, `NavMenu.razor` e `NavMenu.razor.css`, e limpeza do `MainLayout.razor.css` e do `app.css` herdados do template (mantendo o estilo de `#blazor-error-ui` e de validação).

### 8. Formatação em pt-BR
As datas por extenso ("Quinta-feira, 11 de junho de 2026") e os números usam `CultureInfo("pt-BR")` explicitamente nos componentes, com a primeira letra do dia da semana em maiúscula, sem depender da cultura do servidor.

## Risks / Trade-offs

- **[Parâmetro serializado no componente interativo]** Passar DTOs de SSR para `RankingFifaChart` interativo exige tipos serializáveis. → DTOs são `record`s com tipos primitivos. A configuração do Chart.js é montada dentro do componente interativo, não passada como parâmetro.
- **[Data de referência fixa]** Com `2026-06-05` no `appsettings.json`, a seção mostra sempre os mesmos jogos, mesmo depois da Copa. → É o comportamento escolhido. Basta remover a chave para usar a data atual.
- **[Links para rotas inexistentes]** Jogos, Grupos, Seleções, Ranking e Simulador abrem a página "não encontrada" até os próximos changes. → Comportamento esperado e coberto no spec de `portal-layout`.
- **[Imagens externas da FIFA]** Sem rede ou com a API fora do ar, faltam bandeiras e logo. → Fallback textual e ocultação. O layout não depende das imagens.
- **[Offset fixo de Brasília]** Não vale para outros anos ou fusos. → Fica isolado em `HorarioBrasilia`, coerente com o seed, e pode ser trocado depois.
- **[Gráfico em circuito desconectado]** No dispose após a queda do circuito, a chamada JS falha. → `DisposeAsync` captura `JSDisconnectedException`.

## Migration Plan

Sem migração de dados: não há mudanças no schema nem no seed. Implantação: build e execução. Verificação manual:
1. `dotnet build src/PortalCopa26/PortalCopa26.slnx` sem erros nem avisos novos.
2. `dotnet run`: abrir `/` e conferir as seções, os totais (48/12/104/16), os estádios por país (11/2/3), os jogos de 11 e 12/06 e o gráfico Top 10 (de Argentina a Alemanha).
3. Navegar para uma rota inexistente e voltar, conferindo no console do navegador que não há erros de JS.
4. Reduzir a janela abaixo de 768 px e de 992 px para conferir o menu recolhido e o empilhamento.

Rollback: reverter os arquivos do change. Não há estado persistido.
