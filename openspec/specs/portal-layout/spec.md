# portal-layout Specification

## Purpose

Define a estrutura visual compartilhada por todas as páginas do PortalCopa26: barra de navegação superior, tema escuro baseado no protótipo validado, rodapé e exibição de imagens da FIFA (logo e bandeiras) com alternativa quando indisponíveis.

## Requirements

### Requirement: Barra de navegação superior
Todas as páginas SHALL exibir no topo uma barra de navegação com a marca "PortalCopa26" acompanhada do logo da Copa e links, nesta ordem, para Home (`/`), Jogos (`/jogos`), Grupos (`/grupos`), Seleções (`/selecoes`), Ranking (`/ranking`) e Simulador (`/simulador`). A marca SHALL levar a `/`. O link da página atual SHALL aparecer destacado.

#### Scenario: Links da navegação
- **WHEN** qualquer página do portal é exibida
- **THEN** a barra superior mostra os links Home, Jogos, Grupos, Seleções, Ranking e Simulador, nessa ordem

#### Scenario: Link ativo
- **WHEN** o visitante está na página inicial
- **THEN** apenas o link Home aparece destacado como ativo

#### Scenario: Rota ainda não implementada
- **WHEN** o visitante segue um link para uma página que ainda não existe
- **THEN** a aplicação exibe a página "não encontrada", mantendo a barra de navegação e o rodapé

### Requirement: Menu em telas estreitas
Em telas estreitas, os links da navegação SHALL ficar recolhidos atrás de um botão de menu, que os exibe e oculta ao ser acionado. O menu SHALL funcionar sem depender de componentes interativos do servidor.

#### Scenario: Abrir o menu recolhido
- **WHEN** a página é exibida com menos de 768 px de largura e o visitante aciona o botão de menu
- **THEN** a lista de links da navegação é exibida

### Requirement: Tema visual do portal
As páginas SHALL usar o tema escuro do protótipo validado (`..\prototipo\css\site.css`): a mesma paleta (fundo azul-marinho, superfícies e cartões em tons de azul, destaques em azul e dourado, texto claro) e a mesma tipografia. Os estilos SHALL ser construídos sobre o Bootstrap 5, sem desativar seus componentes.

#### Scenario: Tema aplicado
- **WHEN** qualquer página do portal é carregada
- **THEN** o fundo da página usa a cor `#151f2e` e o texto principal usa a cor `#f0f6ff`

#### Scenario: Páginas de template removidas
- **WHEN** um visitante acessa `/counter` ou `/weather`
- **THEN** a aplicação responde com a página "não encontrada"

### Requirement: Rodapé
Todas as páginas SHALL exibir um rodapé com a marca "PortalCopa26", a linha "Copa do Mundo FIFA 2026 · USA · Canadá · México" e links para Jogos, Grupos, Ranking e Simulador.

#### Scenario: Rodapé presente
- **WHEN** qualquer página do portal é exibida
- **THEN** o rodapé com a marca, a linha da Copa e os quatro links aparece ao final da página

### Requirement: Imagens da FIFA com alternativa
O logo da Copa SHALL ser obtido de `https://api.fifa.com/api/v3/picture/tournaments-sq-4/285023`, e a bandeira de cada seleção de `https://api.fifa.com/api/v3/picture/flags-sq-4/<código FIFA>`. Quando uma bandeira não puder ser carregada, o código FIFA da seleção SHALL ser exibido no lugar. Quando o logo não puder ser carregado, ele SHALL ser ocultado sem quebrar o layout.

#### Scenario: Bandeira indisponível
- **WHEN** a imagem da bandeira do Brasil não pode ser carregada
- **THEN** o texto "BRA" é exibido no espaço da bandeira

#### Scenario: Logo indisponível
- **WHEN** o logo da Copa não pode ser carregado
- **THEN** a imagem do logo é ocultada, e a marca textual "PortalCopa26" continua visível
