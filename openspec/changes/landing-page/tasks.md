# Tasks

## 1. Serviços de consulta

- [ ] 1.1 Criar os DTOs em `Models/Dtos/` (`ResumoTorneioDto`, `PaisSedeDto`, `DiaJogosDto`, `JogoResumoDto`, `RankingItemDto`) como `record`s sem referências a EF Core ou UI. Verificar compilando e conferindo que nenhum arquivo em `Models/` usa `Microsoft.EntityFrameworkCore` ou `Microsoft.AspNetCore.Components`
- [ ] 1.2 Criar o helper `HorarioBrasilia` em `Services/` (UTC−3 fixo: `ParaBrasilia` e `InicioDoDiaUtc`). Verificar que `InicioDoDiaUtc(2026-06-14)` resulta em `2026-06-14T03:00Z` e que `ParaBrasilia(2026-06-14T04:00Z)` cai em 14/06 01:00
- [ ] 1.3 Implementar `ITorneioService`/`TorneioService` (`ObterResumoAsync` e `ObterPaisesSedeAsync`, ordem EUA, Canadá e México, com códigos USA/CAN/MEX) usando `IDbContextFactory`, `AsNoTracking` e projeção. Verificar pela página no grupo 4 (48/12/104/16 e 11/2/3 estádios)
- [ ] 1.4 Criar `ProximosJogosOptions` (`DataReferencia`) ligado à seção `PortalCopa:ProximosJogos` e adicionar `"DataReferencia": "2026-06-05"` ao `appsettings.json`. Verificar que a aplicação inicia e que o valor é lido
- [ ] 1.5 Implementar `IJogoService`/`JogoService.ObterProximosJogosAsync(dataReferencia, quantidadeDias)` conforme o design (limite em UTC, ordenação por `DataHoraUtc`, agrupamento pela data em Brasília, descrição da vaga quando a seleção é nula). Verificar pela página: com 2026-06-05, os dias 11/06 (2 jogos) e 12/06 (2 jogos); com 2026-06-13, Austrália x Turquia aparece em 14/06
- [ ] 1.6 Implementar `IRankingFifaService`/`RankingFifaService.ObterTopAsync(quantidade)`, ordenado por posição. Verificar pelo gráfico: 10 itens, de 1. Argentina (1877,72) a 10. Alemanha (1735,77)
- [ ] 1.7 Registrar os três serviços (Scoped), `TimeProvider.System` e as options em `AddPortalCopaServices()`. Verificar que `dotnet build` passa e que a aplicação inicia sem erro de resolução de DI

## 2. Layout e tema do portal

- [ ] 2.1 Criar `wwwroot/css/portal.css` com os tokens do protótipo, o mapeamento para as variáveis do Bootstrap e as variantes `.btn-gold`, `.btn-outline-portal`, `.badge-next` e `.badge-group`. Atualizar `App.razor` (`lang="pt-BR"`, `data-bs-theme="dark"`, link para `portal.css` após o Bootstrap). Verificar no navegador que o fundo é `#151f2e` e o texto `#f0f6ff`
- [ ] 2.2 Criar `Components/Shared/BandeiraSelecao.razor` com fallback do código FIFA via `onerror`. Verificar trocando temporariamente a URL por uma inválida: o código (ex.: "BRA") aparece no lugar da imagem
- [ ] 2.3 Criar `PortalNavbar.razor` (marca com logo, `NavLink`s Home/Jogos/Grupos/Seleções/Ranking/Simulador e menu móvel só com CSS) e `PortalFooter.razor` em `Components/Layout/`, com CSS isolado. Verificar que só Home fica ativo em `/` e que o menu abre abaixo de 768 px
- [ ] 2.4 Reescrever `MainLayout.razor` (navbar, `@Body` e rodapé, mantendo `#blazor-error-ui`) e limpar `MainLayout.razor.css` e `app.css` do template. Verificar que `/` e uma rota inexistente exibem navbar e rodapé
- [ ] 2.5 Remover `Counter.razor`, `Weather.razor`, `NavMenu.razor` e `NavMenu.razor.css`. Verificar que o build passa e que `/counter` e `/weather` exibem a página "não encontrada"

## 3. Componente de gráfico Chart.js reutilizável

- [ ] 3.1 Criar o módulo `wwwroot/js/charts.js` (`criarGrafico`, `atualizarGrafico`, `destruirGrafico`, `WeakMap` por canvas e opção declarativa `formatoNumero: "pt-BR"` aplicada à dica e aos ticks). Verificar que o módulo é servido em `/js/charts.js` e importa sem erro no console
- [ ] 3.2 Criar `Components/Shared/Charts/ChartJs.razor` (parâmetros `Configuracao`, `Altura` e `CssClass`; import do módulo no primeiro render; atualização quando os parâmetros mudam; `IAsyncDisposable` tratando `JSDisconnectedException`). Verificar pelo `RankingFifaChart` no grupo 4 e confirmar que, ao navegar para fora de `/`, não aparecem erros no console

## 4. Componentes da Landing Page

- [ ] 4.1 Criar `Components/Pages/LandingPage/HeroSection.razor` (+ `.razor.css`) com selo, título, descrição, logo, botões para `/simulador` e `/jogos` e cartões dos países-sede recebidos por parâmetro. Verificar visualmente contra o `..\prototipo\index.html` e conferir os textos "País Sede · 11/2/3 estádios"
- [ ] 4.2 Criar `EstatisticasCopa.razor` (+ `.razor.css`) com os quatro indicadores recebidos por `ResumoTorneioDto`. Verificar que mostra 48 Seleções, 12 Grupos, 104 Jogos e 16 Estádios
- [ ] 4.3 Criar `ProximosJogos.razor` (+ `.razor.css`) com o título do dia por extenso em pt-BR, a contagem "N jogos", cartões com bandeira, nome, código, horário, grupo, estádio e cidade, o link "Ver todos" (`/jogos`) e a mensagem para lista vazia. Verificar com 2026-06-05 ("Quinta-feira, 11 de junho de 2026", 2 jogos, México x África do Sul às 16:00) e com uma data após 19/07/2026 (mensagem de nenhum jogo)
- [ ] 4.4 Criar `RankingFifaChart.razor` com a configuração de barras horizontais (cores do pódio, azul, `x.min = 1600`, estilo do protótipo, `formatoNumero: "pt-BR"`) sobre o `ChartJs`, mais o link "Ranking completo" (`/ranking`). Verificar no navegador as 10 barras, as cores ouro, prata e bronze e a dica "1.877,72 pts" na primeira barra
- [ ] 4.5 Criar `SimuladorPainel.razor` (+ `.razor.css`) com "Simule a Copa!", o texto explicativo e os botões "Iniciar Simulação" (`/simulador`) e "Ver Grupos" (`/grupos`). Verificar os destinos dos links
- [ ] 4.6 Reescrever `Components/Pages/Home.razor`: injetar os serviços e as options, carregar os dados com `Task.WhenAll`, compor os cinco componentes na ordem do spec com `RankingFifaChart` em `@rendermode="InteractiveServer"` dentro de `row`/`col-lg-8`/`col-lg-4` e definir o `<PageTitle>`. Verificar que `/` responde HTTP 200 com todas as seções e que o painel do simulador empilha abaixo de 992 px

## 5. Verificação integrada

- [ ] 5.1 Rodar `dotnet build src/PortalCopa26/PortalCopa26.slnx` e verificar 0 erros e nenhum aviso novo
- [ ] 5.2 Rodar a aplicação e percorrer o roteiro do design (Migration Plan, itens 2 a 4): seções e dados, navegação para fora e de volta sem erros de JS e comportamento abaixo de 768 px e 992 px. Verificar também que remover `DataReferencia` do `appsettings.json` faz a seção usar a data atual (hoje, mensagem de nenhum jogo)
