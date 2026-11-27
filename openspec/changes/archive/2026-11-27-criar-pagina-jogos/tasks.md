# Tasks

## 1. Dados e serviços

- [x] 1.1 Criar `Models/PaisSedeExtensions.cs` (`OrdemExibicao`, `Nome()`, `CodigoFifa()`) e fazer o `TorneioService` usá-lo no lugar da lista fixa. Verificar na Home que os países-sede continuam "Estados Unidos · 11", "Canadá · 2" e "México · 3" estádios
- [x] 1.2 Criar `EstadioResumoDto` e trocar `Estadio`/`Cidade` de `JogoResumoDto` por ele; criar `GrupoResumoDto` e `SelecaoGrupoDto` em `Models/Dtos/`. Verificar que `dotnet build` passa e que nenhum DTO referencia EF Core ou UI
- [x] 1.3 Extrair em `JogoService` o método `ObterPorDiaAsync(filtro)` (projeção com país e capacidade do estádio, ordenação por `DataHoraUtc` e `Numero`, agrupamento por dia em Brasília), reescrever `ObterProximosJogosAsync` sobre ele e adicionar `ObterJogosAsync(grupo)` em `IJogoService`. Verificar que a Home ainda mostra 11/06 e 12/06 com 2 jogos cada (data de referência 2026-06-05)
- [x] 1.4 Criar `IGrupoService`/`GrupoService.ObterGruposAsync()` (grupos de A a L, cabeça de chave primeiro) e registrá-lo como Scoped em `AddPortalCopaServices()`. Verificar que a aplicação inicia sem erro de resolução de DI

## 2. Componentes compartilhados

- [x] 2.1 Criar `Components/Shared/FormatoPtBr.cs` (data por extenso, dia e mês, hora, inteiro pt-BR). Verificar pelo título "Quinta-feira, 11 de junho de 2026" na Home
- [x] 2.2 Criar `Components/Shared/JogosDataHeader.razor` (+ `.razor.css`) com a data por extenso e "1 jogo"/"N jogos". Verificar, depois da 2.4, que a Home mostra "Quinta-feira, 11 de junho de 2026" com "2 jogos" (data de referência 2026-06-05)
- [x] 2.3 Criar `Components/Shared/JogoCard.razor` (+ `.razor.css`) com os parâmetros `Jogo`, `Selo` (padrão "Agendado") e `ExibirDetalhesEstadio` (país, capacidade e número do jogo). Verificar, depois da 2.4 e da 3.2, que o jogo 1 mostra "México · Capacidade 87.523 · Jogo 1" em `/jogos` (com `ExibirDetalhesEstadio`) e não mostra essa linha na Home (sem o parâmetro), e que o selo é "Agendado" em `/jogos` e "Próximo" na Home
- [x] 2.4 Mover os estilos compartilhados (`.pc-page-header`, `.pc-toolbar`, `.pc-day-group`, `.pc-game-list`, `.pc-empty`) para `portal.css`, reescrever `LandingPage/ProximosJogos.razor` com `JogosDataHeader` e `JogoCard` (`Selo="Próximo"`) e remover `ProximosJogos.razor.css`. Verificar que a seção "Próximos Jogos" da Home continua atendendo os cenários de `landing-page` (México x África do Sul às 16:00, grupo A, "Estádio Azteca · Cidade do México") com o mesmo visual

## 3. Página de Jogos

- [x] 3.1 Criar `Components/Pages/JogosPage/JogosFiltro.razor` (+ `.razor.css`) com links "Todos" e "Grupo X" a partir dos grupos recebidos, opção ativa com `active` e `aria-current="page"`. Verificar que `/jogos?grupo=c` destaca "Grupo C" e `/jogos` destaca "Todos"
- [x] 3.2 Criar `Components/Pages/Jogos.razor` (+ `.razor.css`) em `/jogos`: título e resumo, `<PageTitle>`, filtro, botão "Ver Grupos" (`/grupos`), aviso de grupo inexistente, painel do grupo filtrado e lista por dia com `JogosDataHeader` e `JogoCard` com detalhes do estádio. Verificar os cenários do spec: `/jogos` (104 jogos em 34 dias, resumo "104 partidas · 11 de junho a 19 de julho de 2026"), `/jogos?grupo=c` (6 jogos; Brasil cabeça de chave, Marrocos, Haiti, Escócia; 5 estádios), `/jogos?grupo=Z` (aviso e todos os jogos), `/jogos?grupo=` (todos os jogos, sem aviso), link "Jogos" da barra de navegação destacado em `/jogos` e em `/jogos?grupo=C` (se não ficar, corrigir a comparação do `NavLink` em `PortalNavbar` neste change), jogo 1 com "México · Capacidade 87.523 · Jogo 1", final com "Final" e "Venc. Semifinal 1 x Venc. Semifinal 2" sem bandeiras, e Coreia do Sul x República Tcheca em 11/06 às 23:00

## 4. Verificação integrada

- [x] 4.1 Rodar `dotnet build src/PortalCopa26/PortalCopa26.slnx` e verificar 0 erros e nenhum aviso novo
- [x] 4.2 Abrir `/jogos` no navegador e conferir o visual contra `..\prototipo\jogos.html`, a troca de filtro pela navegação aprimorada sem recarregar a página, o cartão empilhado abaixo de 640 px e os links "Ver todos" e "Ver Jogos" da Home chegando à nova página

## Workflow follow-up

- Arquivar o change depois da revisão (`openspec archive criar-pagina-jogos`).
