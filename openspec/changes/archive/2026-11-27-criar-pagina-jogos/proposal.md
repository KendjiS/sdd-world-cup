# Proposal

## Why

A Landing Page já tem o link "Ver todos" em "Próximos Jogos" e o botão "Ver Jogos" no hero, e os dois levam a `/jogos`, que hoje abre a página "não encontrada". Falta a consulta completa dos jogos oficiais carregados pelo seed, organizada por dia e filtrável por grupo, com as informações do grupo e do estádio de cada partida.

## What Changes

- Nova página `/jogos` com todos os jogos oficiais registrados no banco (fase de grupos e mata-mata), agrupados por dia e em ordem de data e hora, no horário de Brasília.
- Filtro por grupo (Todos, Grupo A a Grupo L, conforme os grupos do banco). O grupo escolhido fica na URL (`/jogos?grupo=A`), o que permite compartilhar o link. Um grupo inexistente exibe um aviso e todos os jogos.
- Painel de informações do grupo filtrado: as seleções (com o cabeça de chave marcado), a quantidade de jogos e os estádios usados.
- Informações do estádio em cada jogo: nome, cidade, país-sede e capacidade, além do número oficial do jogo.
- Botão "Ver Grupos", que leva a `/grupos`.
- Componentes: `Jogos.razor` (página), `JogosFiltro.razor` (filtro por grupo), `JogosDataHeader.razor` (título de cada dia) e `JogoCard.razor` (cartão do jogo). `JogosDataHeader` e `JogoCard` serão compartilhados e passarão a ser usados também pela seção "Próximos Jogos" da Landing Page, que hoje tem o cartão e o título do dia escritos dentro dela. O comportamento dessa seção não muda.
- Novo serviço de consulta dos grupos com as suas seleções, reutilizável pela futura página de Grupos.

O botão "Ver Grupos" aponta para uma rota que será criada em um próximo change. Até lá, abre a página "não encontrada".

Fora do escopo: resultados e placares (o seed não tem gols), filtro por fase, seleção ou estádio, a página de Grupos e a classificação dos grupos.

## Capabilities

### New Capabilities
- `jogos-page`: listagem completa dos jogos da Copa, com agrupamento por dia, filtro por grupo, informações do grupo filtrado e do estádio de cada jogo.

### Modified Capabilities

(nenhuma; a seção "Próximos Jogos" de `landing-page` passará a usar os componentes compartilhados, mas os seus requisitos não mudam)

## Impact

- `Components/Pages/`: nova `Jogos.razor` (+ `.razor.css`) e nova pasta `JogosPage/` com `JogosFiltro`.
- `Components/Shared/`: novos `JogoCard`, `JogosDataHeader` e um helper de formatação pt-BR. `LandingPage/ProximosJogos.razor` passará a usá-los e o seu `.razor.css` será removido.
- `Services/`: novo `IGrupoService`/`GrupoService`; `IJogoService` ganha um método para listar os jogos com filtro opcional por grupo; os nomes dos países-sede, hoje fixados no `TorneioService`, serão centralizados em `Models/`.
- `Models/Dtos/`: `JogoResumoDto` passará a trazer um DTO de estádio (nome, cidade, país, capacidade) no lugar dos campos `Estadio` e `Cidade`; novos DTOs de grupo e de seleção do grupo.
- `wwwroot/css/portal.css`: estilos compartilhados de cabeçalho de página, barra de ferramentas e listas de jogos por dia.
- Sem alterações no modelo de dados, nas migrations ou no seed.
