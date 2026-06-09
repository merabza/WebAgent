# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Multi-repository build context (read first)

This repo is **not self-contained**. `WebAgent.slnx` references 9 sibling repositories by relative
path (`../ConnectionTools`, `../DatabaseTools`, `../ParametersManagement`, `../SystemTools`,
`../ToolsManagement`, `../WebAgentContracts`, `../WebAgentShared`, `../WebSystemTools`). They must be
cloned **side by side** under a shared parent folder (see README "Getting the source"). Nothing here
builds without them.

Practical consequences:
- Most types used in handlers (`IDatabaseManager`, `AppSettings`, `Error`, `ICommand`/`ICommandHandler`,
  the route constants, request/response DTOs) are **defined in sibling repos**, not here. When a symbol
  isn't found locally, search the sibling repos (e.g. `WebAgentContracts`, `WebAgentShared`,
  `ToolsManagement`, `SystemTools`).
- HTTP route strings and request/response contracts live in **`WebAgentContracts`** (external). Adding
  or changing an endpoint's URL means editing that repo too.

## Common commands

```sh
# Build the whole solution (requires all sibling repos present)
dotnet build WebAgent.slnx

# Build just one project
dotnet build WebAgent/WebAgent.csproj

# Run the agent (listens on http://*:5031; Swagger at /swagger in Development)
dotnet run --project WebAgent/WebAgent.csproj
```

To run against a sibling repo locally (e.g. while iterating on `SystemTools`), build that project
directly: `dotnet build ../SystemTools/SystemTools.SystemToolsShared/SystemTools.SystemToolsShared.csproj`.

### Tests

There are **no test projects in this repository** — `WebAgent.slnx` contains only `WebAgent` and
`LibDatabasesApi`. Unit tests live in the sibling repos (e.g. `DatabaseTools.DbTools.Tests`,
`SystemTools.SystemToolsShared.Tests`), each with its own solution. Run one with:

```sh
dotnet test ../SystemTools/SystemTools.SystemToolsShared.Tests --filter "FullyQualifiedName~SomeTest"
```

### Build is strict

`Directory.Build.props` sets `TreatWarningsAsErrors=true`, `AnalysisMode=All`,
`EnforceCodeStyleInBuild=true`, and runs **SonarAnalyzer.CSharp** on every project. Analyzer warnings
and style violations **fail the build** — they are not optional. Also note `Nullable` is **enabled**
and `ImplicitUsings` is **disabled** (every `using` must be explicit). NuGet versions are pinned
centrally in `Directory.Packages.props` (so `PackageReference` entries carry no `Version`).

## Architecture

WebAgent is an ASP.NET Core (.NET 10, Minimal APIs) agent that runs on a database/app server and
exposes a secured API for managing SQL databases and deployed apps/services. The request flow is a
thin CQRS pipeline:

```
Minimal API endpoint  →  MediatR command  →  command handler  →  OneOf<TResult, Error[]>
```

- **Endpoints** (`LibDatabasesApi/Endpoints/V1/DatabasesEndpoints.cs`, and `ProjectsEndpoints` in the
  external `WebAgentShared`) are static classes with `Use…Endpoints` extension methods that map routes
  onto an authorized `RouteGroupBuilder`. Each handler method: reads the caller from
  `ICurrentUserByApiKey`, emits start/finish progress via `IMessagesDataManager` (pushed to clients
  over **SignalR**), builds a command, `mediator.Send`s it, and `Match`es the `OneOf` result to
  `TypedResults.Ok` / `BadRequest`.
- **`Program.cs`** is the composition root. It wires Serilog, Swagger, API-key identity, SignalR, and
  MediatR, then activates each feature's endpoints via its `Use…` extension (`UseLibProjectsApi`,
  `UseLibDatabasesApi`, `UseSignalRMessagesHub`, …). `AddMediator(...)` registers handlers by scanning
  the assemblies passed to it (`LibProjectsApi` + `LibDatabasesApi` here) — a new handler in a new
  assembly won't be found unless that assembly is added to this call.
- **Error handling**: handlers never throw for expected failures. They return `OneOf<TResult, Error[]>`;
  `Error` values come from static `*Errors` classes (e.g. `DbApiErrors`, `DatabaseApiClientErrors`,
  `ProjectsErrors`). Inspect with `.IsT1` / `.AsT0` / `.AsT1`, or fold with `.Match(...)`.
- **Real work is delegated out**: handlers resolve config via `AppSettings.Create(IConfiguration)`,
  then hand off to `ToolsManagement` / `DatabaseTools` (e.g. `DatabaseManagerCreator.Create(...)`
  returns an `IDatabaseManager`; `BaseBackupRestoreTool` performs backups). Handlers contain
  orchestration, not database logic.

### Naming convention (one operation = a matched quartet)

For each operation the pieces share a stem and live in parallel folders inside `LibDatabasesApi`:

| Concern | Type | Folder |
|---------|------|--------|
| Command | `XxxRequestCommand` (`: ICommand` or `ICommand<T>`) | `CommandRequests/` |
| Handler | `XxxCommandHandler` (`: ICommandHandler<…>`) | `Handlers/` |
| Validator | `XxxCommandValidator` (`: AbstractValidator<XxxRequestCommand>`) | `Validators/` |
| Mapping | `…Mapper` (request DTO → command, via `AdaptTo()`) | `Mappers/` |

Commands are plain DTOs with a `Create(...)` factory (or constructor) and always carry `UserName`.
Validators use FluentValidation plus shared rules from `WebAgentShared.LibProjectsApi.Validators`
(e.g. the custom `.FileName()` rule).

### Adding a new database endpoint (checklist)

1. Add the route constant in `WebAgentContracts/.../V1/Routes/DatabaseApiRoutes.cs` (**external repo**).
2. Add request/response DTOs in `WebAgentContracts` if the endpoint has a body.
3. In `LibDatabasesApi`: add the `XxxRequestCommand`, `XxxCommandHandler`, `XxxCommandValidator`, and a
   mapper if there's a request body.
4. Map it in `DatabasesEndpoints.UseDatabasesEndpoints` (`group.MapGet/MapPost/...`).

## Configuration

Runtime config is `WebAgent/appsettings.json` (overridable via `appsettings.{Environment}.json`, user
secrets, env vars). Handlers read it through `AppSettings.Create(_config)`. The `AppSettings` block
defines named SQL Server connections, file storages, backup-retention "smart schemas", and backup
exchange parameters; `ApiKeys:AppSettingsByApiKey` defines accepted API keys (optionally IP-scoped).
The committed file holds placeholder values only — real keys/passwords belong in user secrets or env
vars. See the README's Configuration table for the meaning of each section.
