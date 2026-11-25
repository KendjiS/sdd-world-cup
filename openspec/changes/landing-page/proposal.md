# Proposal

## Why

A fundação do PortalCopa26 (banco SQLite, EF Core, entidades e seed com os dados oficiais) está pronta, mas a URL raiz ainda mostra a página "Hello, world!" do template Blazor, com o menu lateral de exemplo (Counter/Weather). A Landing Page é a porta de entrada do portal e a primeira tela que consome os dados do banco. Ela deve reproduzir o protótipo HTML validado em `..\prototipo\index.html`.

## What Changes

- Nova página inicial (`/`) composta por cinco componentes reutilizáveis em `Components/Pages/LandingPage/`: `HeroSection`, `EstatisticasCopa`, `ProximosJogos`, `RankingFifaChart` e `SimuladorPainel`.
- **Hero Section**: título, descrição, logo da Copa e botões "Abrir Simulador" e "Ver Jogos". Inclui os cartões dos três países-sede com bandeira e número de estádios de cada um, calculado pelo banco.
- **Estatísticas da Copa**: quantidade de seleções, grupos, jogos e estádios, lida do banco.
- **Próximos Jogos**: jogos dos dois próximos dias com partidas a partir de uma data de referência configurável em `appsettings.json`, agrupados por dia. Os horários são exibidos em horário de Brasília.
- **Ranking FIFA (Top 10)**: gráfico de barras horizontais com Chart.js via JSInterop, com destaque em ouro, prata e bronze para os três primeiros.
- **Chamada para o Simulador**: painel com links para o Simulador e para os Grupos.
- Componente de gráfico Chart.js genérico e reutilizável (módulo JS + componente Blazor), para que futuros gráficos estatísticos usem a mesma integração.
- Serviços de consulta para os dados da Landing Page, registrados por injeção de dependência. Os componentes não acessam o EF Core diretamente.
- Layout do portal no visual do protótipo: barra de navegação superior escura com logo e links (Home, Jogos, Grupos, Seleções, Ranking, Simulador), tema escuro e rodapé. Substitui o layout de sidebar do template.
- **BREAKING (template):** remoção das páginas de exemplo `Counter` e `Weather` e do `NavMenu` do template.

Os links para Jogos, Grupos, Seleções, Ranking e Simulador apontam para rotas que serão criadas nos próximos changes. Até lá, abrem a página "não encontrada".

Fora do escopo: páginas Jogos, Grupos, Seleções, Ranking completo e Simulador, navegação avançada, responsividade avançada e implementação do simulador.

## Capabilities

### New Capabilities
- `landing-page`: conteúdo e comportamento da página inicial: hero com países-sede, estatísticas da Copa, próximos jogos, gráfico do Ranking FIFA Top 10 e chamada para o simulador.
- `portal-layout`: estrutura visual compartilhada por todas as páginas: navegação superior, tema escuro, rodapé e imagens da FIFA com fallback.

### Modified Capabilities

(nenhuma; os requisitos de `app-foundation`, `data-model` e `seed-data` não mudam)

## Impact

- `src/PortalCopa26/PortalCopa26/Components/`: novo `Layout/MainLayout`, nova `Pages/Home.razor`, nova pasta `Pages/LandingPage/`, novo componente de gráfico compartilhado. Remoção de `Counter.razor`, `Weather.razor` e `NavMenu.razor`.
- `Services/`: novos serviços de consulta e DTOs. Registro em `Extensions/ServiceCollectionExtensions.cs`.
- `wwwroot/`: tema CSS do portal portado do protótipo e módulo JS de integração com Chart.js. O Chart.js local já existe.
- `appsettings.json`: nova seção com a data de referência dos próximos jogos.
- Sem alterações no modelo de dados, nas migrations ou no seed.
- Dependência de rede apenas para imagens da API pública da FIFA (bandeiras e logo), já previstas no CLAUDE.md, com fallback quando indisponíveis.
