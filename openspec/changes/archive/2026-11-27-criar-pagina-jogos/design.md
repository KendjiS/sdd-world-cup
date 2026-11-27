# Design

## Context

- A Landing Page (change `2026-11-25-landing-page`) criou `IJogoService.ObterProximosJogosAsync`, o helper `HorarioBrasilia` (UTC−3 fixo) e o componente `ProximosJogos`, que tem o cartão de jogo e o título do dia escritos dentro dele, com o CSS em `ProximosJogos.razor.css`.
- As páginas são renderizadas em static SSR. Só o gráfico do ranking usa `InteractiveServer`.
- `JogoResumoDto` traz o estádio como dois textos (`Estadio`, `Cidade`); o país e a capacidade, que existem em `Estadio`, não chegam à UI.
- O seed não grava gols: `Jogo.GolsMandante`/`GolsVisitante` são sempre nulos.
- Os nomes dos países-sede estão fixados dentro do `TorneioService`.
- Não existe serviço de grupos.

Requisitos: ver `specs/jogos-page/spec.md`. Motivação: ver `proposal.md`.

## Goals / Non-Goals

**Goals:**
- Página de Jogos em static SSR, sem circuito SignalR.
- Um único cartão de jogo e um único título de dia para a Home e para a página de Jogos.
- Serviço de grupos reutilizável pela futura página de Grupos.

**Non-Goals:**
- Placar e situação real dos jogos (sem dados no seed). O cartão mostra "— : —".
- Testes automatizados (o projeto ainda não tem projeto de testes). A verificação é por build e execução, como no change da Landing Page.

## Decisions

### 1. Filtro por query string, não por interatividade
`JogosFiltro` renderiza links (`jogos` e `jogos?grupo=X`), e `Jogos.razor` lê o grupo com `[SupplyParameterFromQuery(Name = "grupo")]` em `OnParametersSetAsync`. A navegação aprimorada do Blazor troca o conteúdo sem recarregar a página inteira.

- Um valor vazio ou só com espaços é tratado como "Todos", sem aviso.
- O link "Jogos" da `PortalNavbar` usa `NavLinkMatch.Prefix`; a tarefa 3.2 confere que ele continua ativo com `?grupo=`.
- *Por quê:* o filtro funciona em static SSR, sem circuito por visitante, e a URL pode ser compartilhada ou favoritada.
- *Alternativa descartada:* `InteractiveServer` com botões e estado em memória. Exigiria circuito, faria a consulta duas vezes por causa do prerender e perderia o link compartilhável.

### 2. Grupo validado contra o banco
A página carrega os grupos por `IGrupoService.ObterGruposAsync()` e procura o código recebido sem diferenciar maiúsculas de minúsculas. Se não existir, exibe o aviso e consulta sem filtro. A mesma lista alimenta as opções do filtro, sem letras fixadas no código.

- *Por quê:* segue a regra do CLAUDE.md de não criar grupos não definidos; o filtro reflete exatamente o que o seed carregou.

### 3. Uma consulta de jogos com filtro opcional
`JogoService` ganha `ObterJogosAsync(string? grupo)`. Ela e `ObterProximosJogosAsync` passam a usar o mesmo método privado `ObterPorDiaAsync(Expression<Func<Jogo,bool>>? filtro)`, que projeta para DTO, ordena por `DataHoraUtc` e `Numero` e agrupa por dia em Brasília em memória (no máximo 104 jogos). `ObterProximosJogosAsync` aplica o filtro de data e faz `Take(quantidadeDias)` sobre os dias.

- *Por quê:* evita duplicar a projeção e a regra de fuso; a ordenação por `Numero` desempata jogos simultâneos (última rodada dos grupos).
- *Alternativa descartada:* um método separado com cópia da projeção.

### 4. Estádio como DTO próprio
`JogoResumoDto.Estadio` passa a ser `EstadioResumoDto(Nome, Cidade, Pais, Capacidade)`. O nome de exibição do país vem de `PaisSedeExtensions.Nome()` (em `Models/`), que também passa a ser usado pelo `TorneioService` (com `OrdemExibicao` e `CodigoFifa()`).

- *Por quê:* agrupa os dados do estádio e elimina a segunda cópia dos nomes de países.
- *Trade-off:* muda a assinatura do record; só `JogoService` e os componentes de jogo o constroem ou leem, e todos são atualizados juntos.

### 5. Componentes compartilhados em `Components/Shared/`
- `JogoCard` (parâmetros `Jogo`, `Selo` padrão "Agendado", `ExibirDetalhesEstadio`) e `JogosDataHeader` (`Data`, `Quantidade`) ficam em `Shared/`, porque a Home também os usa. A Home passa `Selo="Próximo"` e não exibe os detalhes do estádio, mantendo os requisitos de `landing-page` inalterados.
- `JogosFiltro` é específico da página e fica em `Components/Pages/JogosPage/`. A pasta não se chama `Jogos/` porque o namespace `PortalCopa26.Components.Pages.Jogos` colidiria com a classe da página `Jogos`.
- Formatações pt-BR (data por extenso, dia e mês, hora, inteiro com separador de milhar) ficam em `Shared/FormatoPtBr.cs`.
- Os estilos usados por mais de uma página (`.pc-page-header`, `.pc-toolbar`, `.pc-day-group`, `.pc-game-list`, `.pc-empty`) vão para `portal.css`; os estilos do cartão e do título ficam no CSS isolado de cada componente. `ProximosJogos.razor.css` é removido.

### 6. Grupos com as seleções em um serviço próprio
`IGrupoService.ObterGruposAsync()` devolve `GrupoResumoDto(Codigo, Selecoes)` em ordem alfabética, com `SelecaoGrupoDto(Nome, CodigoFifa, CabecaDeChave)` ordenadas com o cabeça de chave primeiro e depois por `Id`. O `Id` reflete a ordem de inserção do seed, que segue a ordem de `selecoes.json` (gerado a partir de `copa2026_grupos.txt`); o modelo não tem um campo de posição no grupo, e esta change não cria um.

- *Por quê:* serviço por assunto, como os da Landing Page; a página de Grupos vai reutilizá-lo.

### 7. Informações do grupo calculadas na página
O painel do grupo usa as seleções de `GrupoResumoDto` e calcula a quantidade de jogos e os estádios distintos a partir dos jogos já carregados, sem consulta extra.

## Risks / Trade-offs

- [O botão "Ver Grupos" leva a uma rota ainda inexistente] → Aceito, como nos links da Landing Page; a página de Grupos vem em um próximo change.
- [Com o filtro em "Todos", a página renderiza 104 cartões de uma vez] → Volume pequeno para HTML estático; sem paginação nem virtualização.
- [Bandeiras da API da FIFA indisponíveis] → `BandeiraSelecao` já exibe o código FIFA como alternativa.

## Migration Plan

1. `dotnet build src/PortalCopa26/PortalCopa26.slnx` sem erros nem avisos novos.
2. Rodar a aplicação e conferir `/jogos`, `/jogos?grupo=c`, `/jogos?grupo=Z` e `/` contra os cenários do spec e os de "Próximos Jogos" em `landing-page`.
3. Rollback: reverter o change; não há migration nem alteração de dados.
