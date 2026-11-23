## Purpose

Define como a solução PortalCopa26 é estruturada e hospedada: onde fica o código-fonte, como a aplicação web inicia, como o código é organizado para permitir futura migração para camadas e quais assets de front-end compartilhados ficam disponíveis para as páginas futuras.

## ADDED Requirements

### Requirement: Estrutura da solução
Todo o código-fonte SHALL ficar em `src/` na raiz do workspace, organizado como `src/PortalCopa26/PortalCopa26.slnx` (solução no formato `.slnx`) e um único projeto de aplicação em `src/PortalCopa26/PortalCopa26/`.

#### Scenario: Solução compila
- **WHEN** um desenvolvedor compila `src/PortalCopa26/PortalCopa26.slnx`
- **THEN** a compilação termina sem erros
- **AND** a solução contém exatamente um projeto de aplicação, `PortalCopa26`

#### Scenario: Projeto usa .NET 10
- **WHEN** o arquivo de projeto é inspecionado
- **THEN** o target framework é `net10.0`

### Requirement: Hospedagem da aplicação web
A aplicação SHALL executar como Blazor Web App com renderização interativa no servidor e SHALL servir uma página inicial na URL raiz quando iniciada localmente.

#### Scenario: Aplicação inicia e serve a página inicial
- **WHEN** a aplicação é iniciada com `dotnet run`
- **THEN** uma requisição para `/` retorna HTTP 200 com uma página HTML

#### Scenario: Componentes interativos funcionam
- **WHEN** uma página com um componente interativo é aberta no navegador
- **THEN** as interações do usuário com o componente são processadas sem recarregar a página inteira

### Requirement: Organização do código para futura separação em camadas
O projeto SHALL separar o código em `Components/` (layouts, páginas e componentes reutilizáveis), `Models/` (entidades e enums de domínio), `Services/` (serviços de aplicação e suas interfaces) e `Data/` (contexto de persistência, mapeamentos, migrations e seed). As entidades em `Models/` SHALL NOT depender de código de persistência ou de UI.

#### Scenario: Pastas existem
- **WHEN** o diretório do projeto é listado
- **THEN** as pastas `Components`, `Models`, `Services` e `Data` estão presentes

#### Scenario: Entidades sem dependência de persistência
- **WHEN** os arquivos em `Models/` são inspecionados
- **THEN** nenhum deles referencia namespaces do framework de persistência ou de UI

### Requirement: Registro de dependências
Serviços de aplicação e persistência SHALL ser registrados no contêiner nativo de injeção de dependência do ASP.NET Core, agrupados em métodos de extensão dedicados em vez de declarados diretamente no arquivo de inicialização. Componentes e serviços SHALL obter suas dependências por injeção.

#### Scenario: Persistência resolvida pelo contêiner
- **WHEN** a aplicação termina de iniciar
- **THEN** o contexto de persistência pode ser obtido do contêiner de serviços sem construção manual

### Requirement: Assets de front-end compartilhados
A aplicação SHALL disponibilizar Bootstrap 5 e Chart.js para todas as páginas como arquivos estáticos servidos localmente, sem depender de CDNs externas em tempo de execução.

#### Scenario: Chart.js disponível para gráficos futuros
- **WHEN** qualquer página é carregada no navegador
- **THEN** a biblioteca Chart.js é carregada dos arquivos estáticos da própria aplicação
- **AND** o objeto global `Chart` está definido na página

#### Scenario: Estilos do Bootstrap aplicados
- **WHEN** qualquer página é carregada no navegador
- **THEN** a folha de estilos do Bootstrap 5 é carregada dos arquivos estáticos da própria aplicação
