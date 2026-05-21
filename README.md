# Azure Todo Examensprojekt

En modern ToDo-applikation för examensprojektet **Cloudutvecklare Azure**. Syftet är att visa hur en molnbaserad webbapplikation kan designas, utvecklas och driftsättas i Microsoft Azure med fokus på Clean Architecture, säker utveckling och automatiserad CI/CD.

## Teknisk stack

- .NET 8 och Visual Studio 2022
- ASP.NET Core MVC frontend
- ASP.NET Core REST API
- Entity Framework Core
- ASP.NET Core Identity
- Azure SQL Database
- Azure App Service
- Azure DevOps YAML pipeline
- xUnit och Moq

## Clean Architecture

```text
Todo.Domain
  Entities och domänregler

Todo.Application
  DTOs, interfaces, service layer och use cases

Todo.Infrastructure
  EF Core, Identity, DbContext och repository implementation

Todo.API
  REST endpoints: /api/todos

Todo.Web
  ASP.NET Core MVC dashboard och Identity UI

Todo.Tests
  Unit tests för service-lagret
```

Flödet är:

```text
MVC / API -> Application services -> Repository interface -> Infrastructure EF Core -> Azure SQL
```

## Funktioner

- Registrera konto, logga in och logga ut med ASP.NET Core Identity
- Skapa, läsa, uppdatera, markera som klar och ta bort todos
- Filtrering på alla, aktiva och klara todos
- Dashboard med statistik
- Användarisolering: varje användare ser endast sina egna todos
- REST API med skyddade endpoints
- Modern dashboarddesign inspirerad av Microsoft To Do, Notion och Azure

## REST API

Skyddade endpoints i `Todo.API`:

- `GET /api/todos?status=All`
- `GET /api/todos/{id}`
- `POST /api/todos`
- `PUT /api/todos/{id}`
- `PATCH /api/todos/{id}/complete`
- `DELETE /api/todos/{id}`

## Kör lokalt i Visual Studio 2022

1. Öppna `AzureTodoExamensprojekt.sln`.
2. Kontrollera connection string i `Todo.Web/appsettings.Development.json`.
3. Kör migrationer mot LocalDB:

```powershell
dotnet ef database update --project Todo.Infrastructure --startup-project Todo.Web
```

4. Starta `Todo.Web`.
5. Registrera en användare och börja skapa todos.

För API-testning kan `Todo.API` startas som separat startup project och testas via Swagger i development-läge.

## Azure SQL

Skapa en Azure SQL Database och lägg connection string som App Service Configuration:

```text
Name: ConnectionStrings__DefaultConnection
Value: Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<database>;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

Connection string ska inte hårdkodas i kod. Lokalt används `appsettings.Development.json`; i Azure används App Service Configuration.

## Azure App Service deployment

1. Skapa Resource Group.
2. Skapa Azure SQL Server och Azure SQL Database.
3. Skapa Azure App Service för .NET 8.
4. Lägg in `ConnectionStrings__DefaultConnection` i App Service Configuration.
5. Kör migrationer från utvecklingsmiljö eller som release-steg:

```powershell
dotnet ef database update --project Todo.Infrastructure --startup-project Todo.Web
```

6. Publicera `Todo.Web` till App Service.

## Azure DevOps CI/CD

`azure-pipelines.yml` innehåller:

1. Restore
2. Build
3. Run tests
4. Publish
5. Deploy till Azure App Service

Uppdatera variablerna `azureSubscription` och `webAppName` i pipeline-filen så att de matchar din Azure DevOps service connection och App Service.

## Tester

Kör tester:

```powershell
dotnet test AzureTodoExamensprojekt.sln
```

Tester täcker:

- skapa todo
- uppdatera todo
- filtrering
- att användare bara får sina egna todos

## Screenshots

Lägg gärna in bilder i presentationen:

```text
docs/screenshots/dashboard-smoke.png
docs/screenshots/dashboard.png
docs/screenshots/edit-todo.png
docs/screenshots/swagger-api.png
docs/screenshots/azure-app-service.png
docs/screenshots/devops-pipeline.png
```

## Azure AI Search som vidareutveckling

Projektet innehåller en förberedd interface-struktur i `Todo.Application`:

```text
IAzureAiSearchTodoIndexer
```

Den kan senare implementeras i Infrastructure för att indexera todos, dokumentation eller bilagor i Azure AI Search. Det är medvetet lämnat som framtida vidareutveckling så att examensprojektets huvudfokus förblir Azure App Service, Azure SQL och Azure DevOps CI/CD.
