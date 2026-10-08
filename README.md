# Recommenda — CP2 de .NET

Persistência de um catálogo de filmes e séries com Entity Framework Core e MySQL, organizada em Domain, Application, Infrastructure e API.

| Integrante | RM |
| --- | --- |
| Guilherme Soares | 568227 |
| Gabriel Stuani | 566682 |


## Domínio e base do projeto

O Recommenda permite representar filmes, séries, temporadas, episódios, gêneros, usuários, configurações de usuário e avaliações. Filmes e séries herdam de `Content` e compartilham uma tabela pelo mapeamento TPH.

Esta entrega utiliza o projeto Recommenda fornecido para a turma como base. A CP1 anterior do grupo tratava de e-commerce; para esta CP2, o domínio foi adaptado às exigências específicas do enunciado recebido, que pede Recommenda e TPH de `Movie`/`Serie`.

## Organização

```text
Recommenda.slnx
Recommenda.Domain/             Entidades e tipos do domínio
Recommenda.Application/        Contratos de repositório e serviço de gêneros
Recommenda.Infrastructure/     DbContext, Fluent API, repositório e migrations
Recommenda.API/                Configuração, DI e endpoints HTTP
docs/                         Documentação do esquema físico
tests/Recommenda.IntegrationTests/  Testes de integração com MySQL real
scripts/verify-api.ps1         Verificação HTTP do CRUD
.config/dotnet-tools.json     Versão local da ferramenta dotnet-ef
```

O `RecommendaContext` e todos os mapeamentos `IEntityTypeConfiguration<T>` ficam na Infrastructure. A estratégia de acesso a dados é um repositório genérico assíncrono, com contrato `IRepository<T>` na Application e implementação na Infrastructure. O `DbContext` e o repositório são registrados com ciclo de vida `Scoped` na API.

Os repositórios fazem exclusão física e retornam os registros sem aplicar um filtro implícito a `Active`. Os campos de auditoria usam UTC. As versões NuGet estão fixadas nos projetos e nos arquivos `packages.lock.json`; a ferramenta local `dotnet-ef` usa a mesma versão do EF Core.

## Requisitos

- SDK .NET 10.
- Docker Desktop com o mecanismo de containers em execução.
- Porta 3306 disponível para MySQL e porta 5080 para a API.

O SGBD utilizado é **MySQL**, com o provider `MySql.EntityFrameworkCore` e EF Core 10. Execute os comandos a seguir na raiz do repositório.

## Executar

### 1. Restaurar os pacotes e a ferramenta de migrations

```powershell
dotnet restore
dotnet tool restore
```

### 2. Subir o MySQL

A configuração Docker Compose usa MySQL 8.4.11, container `TDSPB`, porta 3306 e um volume para preservar os dados:

```powershell
docker compose up -d --wait
```

Como alternativa, o comando original do enunciado também pode ser utilizado:

```powershell
docker run --name TDSPB -e MYSQL_ROOT_PASSWORD=TDSPB123 -p 3306:3306 -d mysql:latest
```

Se esse container já existir, inicie-o:

```powershell
docker start TDSPB
```

Escolha uma das formas de criação, pois ambas utilizam o mesmo nome de container e a mesma porta. O Compose fixa a versão em `mysql:8.4.11`; `mysql:latest` segue a imagem publicada mais recentemente. Essa diferença deixa a execução pelo Compose reproduzível. Ao utilizar `docker run` ou `docker start`, aguarde o MySQL terminar a inicialização antes de aplicar a migration.

A connection string de desenvolvimento em `Recommenda.API/appsettings.Development.json` é:

```json
{
  "ConnectionStrings": {
    "MySql": "Server=127.0.0.1;Port=3306;Database=Recommenda;User=root;Password=TDSPB123;"
  }
}
```

`TDSPB123` é a senha pública de desenvolvimento indicada no enunciado. Credenciais reais devem ser fornecidas por configuração externa, como a variável de ambiente `ConnectionStrings__MySql`, e nunca commitadas.

### 3. Aplicar a migration

```powershell
dotnet ef database update --project Recommenda.Infrastructure --startup-project Recommenda.API -- --environment Development
```

A entrega contém uma única migration, `InitialCreate`, que cria o esquema completo. As três migrations de demonstração da aula foram substituídas por essa migration inicial, gerada a partir do modelo final. O comando é destinado a um banco `Recommenda` novo, sem o histórico das migrations da demonstração.

### 4. Iniciar a API

```powershell
dotnet run --project Recommenda.API -- --environment Development --urls http://localhost:5080
```

## Conferir a persistência pela API

A API disponibiliza CRUD de gêneros em `http://localhost:5080/api/genres`. O exemplo demonstra o repositório genérico; o modelo completo é criado pela migration.

| Método | Rota | Resultado esperado |
| --- | --- | --- |
| GET | `/api/genres` | Lista de gêneros |
| GET | `/api/genres/{id}` | Gênero existente ou 404 |
| POST | `/api/genres` | Criação e resposta 201 |
| PUT | `/api/genres/{id}` | Atualização ou 404 |
| DELETE | `/api/genres/{id}` | Exclusão ou 404 |

O corpo de POST e PUT contém `name` obrigatório, com até 100 caracteres, e `description` opcional, com até 500 caracteres. O nome do gênero é único no banco.

Em outro terminal PowerShell, com a API em execução:

Os comandos indicam `charset=utf-8` para preservar os acentos também no Windows PowerShell 5.1.

```powershell
$genre = Invoke-RestMethod -Method Post -Uri 'http://localhost:5080/api/genres' -ContentType 'application/json; charset=utf-8' -Body '{"name":"Ficção científica","description":"Filmes e séries de ficção científica"}'
Invoke-RestMethod -Uri 'http://localhost:5080/api/genres'
Invoke-RestMethod -Uri "http://localhost:5080/api/genres/$($genre.id)"
Invoke-RestMethod -Method Put -Uri "http://localhost:5080/api/genres/$($genre.id)" -ContentType 'application/json; charset=utf-8' -Body '{"name":"Ficção científica","description":"Descrição atualizada"}'
Invoke-RestMethod -Method Delete -Uri "http://localhost:5080/api/genres/$($genre.id)"
```

## Esquema físico

A migration cria oito tabelas de domínio: `Content`, `Genre`, `ContentGenre`, `Season`, `Episode`, `User`, `UserConfiguration` e `Rating`, além da tabela de histórico do EF Core. O [diagrama e as regras de mapeamento](docs/esquema-fisico.md) detalham TPH, cardinalidade, campos opcionais, índices e exclusões em cascata.

O [SQL gerado pela migration](docs/schema.sql) permite inspecionar o esquema. Para instalar ou atualizar o banco, utilize o comando `dotnet ef database update` descrito acima.

## Verificação automatizada

Com o MySQL em execução, execute os testes de integração:

```powershell
$env:RECOMMENDA_TEST_CONNECTION = 'Server=127.0.0.1;Port=3306;User=root;Password=TDSPB123;'
dotnet test Recommenda.slnx --configuration Release
```

Os testes exigem essa variável e criam um banco exclusivo `RecommendaCp2Tests_<identificador>`. Aplicam a migration, verificam os relacionamentos e restrições e removem apenas esse banco temporário ao terminar. O banco `Recommenda` da aplicação é preservado.

Com a API iniciada em Development, o script HTTP verifica criação, leitura, atualização, exclusão, validação, duplicidade, saúde e OpenAPI:

```powershell
pwsh -File scripts/verify-api.ps1
```

Esse script requer PowerShell 7, cria um gênero com nome exclusivo e remove somente esse registro ao terminar. Os [resultados da validação](docs/validacao.md) registram o ambiente e as verificações executadas.
