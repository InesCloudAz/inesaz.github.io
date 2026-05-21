# Azure Todo Examensprojekt

En modern ToDo-applikation for examensprojektet **Cloudutvecklare Azure**. Syftet ar att visa hur en molnbaserad webbapplikation kan designas, utvecklas och driftsattas i Microsoft Azure med fokus pa Clean Architecture, saker utveckling och automatiserad CI/CD.

## Teknisk Stack

- .NET 8 och Visual Studio 2022
- ASP.NET Core MVC frontend
- ASP.NET Core REST API
- Entity Framework Core
- ASP.NET Core Identity
- Azure SQL Database
- Azure App Service
- Azure AI Search
- Azure DevOps YAML pipeline
- xUnit och Moq

## Clean Architecture

```text
Todo.Domain
  Entities och domanregler

Todo.Application
  DTOs, interfaces, service layer och use cases

Todo.Infrastructure
  EF Core, Identity, DbContext, repository och Azure AI Search-adapter

Todo.API
  REST endpoints: /api/todos

Todo.Web
  ASP.NET Core MVC dashboard och Identity UI

Todo.Tests
  Unit tests for service-lagret
```

Flodet ar:

```text
MVC / API -> Application services -> Repository interfaces -> Infrastructure -> Azure SQL / Azure AI Search
```

## Funktioner

- Registrera konto, logga in och logga ut med ASP.NET Core Identity
- Skapa, lasa, uppdatera, markera som klar och ta bort todos
- Filtrering pa alla, aktiva och klara todos
- Dashboard med statistik och sokruta
- Anvandarisolering: varje anvandare ser endast sina egna todos
- REST API med skyddade endpoints
- Valfri Azure AI Search-integration med fallback till databassokning
- Modern dashboarddesign inspirerad av Microsoft To Do, Notion och Azure

## REST API

Skyddade endpoints i `Todo.API`:

- `GET /api/todos?status=All`
- `GET /api/todos?searchTerm=azure`
- `GET /api/todos/{id}`
- `POST /api/todos`
- `PUT /api/todos/{id}`
- `PATCH /api/todos/{id}/complete`
- `DELETE /api/todos/{id}`

## Kor Lokalt I Visual Studio 2022

1. Oppna `AzureTodoExamensprojekt.sln`.
2. Kontrollera connection string i `Todo.Web/appsettings.Development.json`.
3. Kor migrationer mot LocalDB:

```powershell
dotnet ef database update --project Todo.Infrastructure --startup-project Todo.Web
```

4. Starta `Todo.Web`.
5. Registrera en anvandare och borja skapa todos.

For API-testning kan `Todo.API` startas som separat startup project och testas via Swagger i development-lage.

## Azure SQL

Skapa en Azure SQL Database och lagg connection string som App Service Configuration:

```text
Name: ConnectionStrings__DefaultConnection
Value: Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<database>;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

Connection string ska inte hardkodas i kod. Lokalt anvands `appsettings.Development.json`; i Azure anvands App Service Configuration.

## Azure AI Search

Projektet innehaller en enkel och valfri Azure AI Search-integration. Den anvands for att indexera anvandarens todos och gora fritextsokning pa titel och beskrivning. Om Azure AI Search inte ar konfigurerat fungerar applikationen fortfarande och faller tillbaka till vanlig databassokning via Entity Framework Core.

Application-lagret innehaller kontrakten:

```text
IAzureAiSearchTodoIndexer
IAzureAiSearchTodoSearchService
TodoSearchDocument
```

Infrastructure-lagret innehaller en SDK-forberedd implementation med `Azure.Search.Documents`. Todo-service anropar indexering nar todos skapas, uppdateras, markeras som klara eller tas bort. Fel i Azure Search loggas som varningar och stoppar inte huvudflodet.

### Skapa Search Service

1. Skapa en Azure AI Search Service i samma resource group som App Service och Azure SQL.
2. Valj en prisniva som passar demo/examensprojekt, till exempel Free om den ar tillganglig.
3. Skapa ett index, exempelvis `todos`.
4. Lagg till falt som matchar `TodoSearchDocument`:

```text
Id          Edm.String   Key, Filterable
UserId      Edm.String   Filterable
Title       Edm.String   Searchable
Description Edm.String   Searchable
IsCompleted Edm.Boolean  Filterable
CreatedAt   Edm.DateTimeOffset Sortable
UpdatedAt   Edm.DateTimeOffset Sortable
```

### App Settings

Lagg in dessa i Azure App Service Configuration. Anvand dubbel underscore for nested configuration:

```text
AzureSearch__Endpoint=https://<search-service-name>.search.windows.net
AzureSearch__IndexName=todos
AzureSearch__ApiKey=<admin-or-query-key>
```

Inga riktiga nycklar ska laggas i koden eller i Git. Lokalt kan vardena lamnas tomma i `appsettings.json`, vilket gor att databassokning anvands.

### Vidareutveckling

En naturlig vidareutveckling ar att skapa indexet automatiskt fran kod eller pipeline, anvanda separata query/admin-nycklar och lagga till sokfilter for status och datum. Ingen RAG eller OpenAI behovs for detta projekt.

## Azure App Service Deployment

1. Skapa Resource Group.
2. Skapa Azure SQL Server och Azure SQL Database.
3. Skapa Azure App Service for .NET 8.
4. Lagg in `ConnectionStrings__DefaultConnection` i App Service Configuration.
5. Lagg eventuellt in `AzureSearch__Endpoint`, `AzureSearch__IndexName` och `AzureSearch__ApiKey`.
6. Kor migrationer fran utvecklingsmiljo eller som release-steg:

```powershell
dotnet ef database update --project Todo.Infrastructure --startup-project Todo.Web
```

7. Publicera `Todo.Web` till App Service.

## Azure DevOps CI/CD

`azure-pipelines.yml` innehaller:

1. Restore
2. Build
3. Run tests
4. Publish
5. Deploy till Azure App Service

Uppdatera variablerna `azureSubscription` och `webAppName` i pipeline-filen sa att de matchar din Azure DevOps service connection och App Service.

## Tester

Kor tester:

```powershell
dotnet test AzureTodoExamensprojekt.sln
```

Tester tacker:

- skapa todo
- uppdatera todo
- filtrering
- att anvandare bara far sina egna todos
- fallback till databassokning nar Azure AI Search inte ar konfigurerat

## Screenshots

Lagg garna in bilder i presentationen:

```text
docs/screenshots/dashboard-smoke.png
docs/screenshots/dashboard.png
docs/screenshots/edit-todo.png
docs/screenshots/swagger-api.png
docs/screenshots/azure-app-service.png
docs/screenshots/devops-pipeline.png
```
