# WebAgent

🌐 **[English](#english)** | **[ქართული](#ქართული)**

---

<a name="english"></a>

## English

**WebAgent** is a lightweight, remotely-controllable agent (ASP.NET Core Web API) that runs on a
database / application server and exposes a secured HTTP API for managing **SQL databases** and
**deployed applications / Windows services** on that machine.

It is the server-side counterpart that remote management tools call into to perform operations such
as creating and restoring database backups, checking and repairing databases, deploying and updating
applications, and starting/stopping services — all over an API-key–protected REST API with real-time
progress reporting over SignalR.

### Features

#### Database management (`api/v1/databases`)
- Create full database backups and exchange them through a configured file storage
- Restore databases from backups
- Check & repair databases
- Update statistics and recompile stored procedures
- Execute arbitrary SQL commands
- Query database names, connection names and folder-set names
- Test the database server connection and check whether a database exists

#### Project / service management (`api/v1/projects`)
- Deploy / update applications and Windows services
- Start and stop Windows services
- Remove a deployed project or service
- Update a deployed application's settings
- Query the running version and the app-settings version of a deployed application

#### Platform
- API-key based authentication, scoped per remote IP address
- Real-time operation progress messages over **SignalR**
- **Swagger / OpenAPI** UI for exploring and testing the API
- **Serilog** structured logging (console + rolling file)
- Can run as a console application or as a **Windows Service**

### Tech stack

| Area | Technology |
|------|------------|
| Runtime | .NET 10.0 / ASP.NET Core (Minimal APIs) |
| Messaging / CQRS | [MediatR](https://github.com/jbogard/MediatR) (`ICommand` / `ICommandHandler`) |
| Validation | [FluentValidation](https://fluentvalidation.net/) |
| Result handling | [OneOf](https://github.com/mcintyre321/OneOf) discriminated unions (`OneOf<TResult, Error[]>`) |
| Logging | [Serilog](https://serilog.net/) |
| Real-time | ASP.NET Core SignalR |
| API docs | Swagger / OpenAPI |
| Banner | Figgle.Fonts (ASCII startup banner) |
| Code quality | Nullable enabled, `TreatWarningsAsErrors`, SonarAnalyzer.CSharp, full analysis mode |

### Architecture

The application follows a thin **endpoint → MediatR command → handler** pipeline:

```
HTTP request
   │
   ▼
Minimal API endpoint (DatabasesEndpoints / ProjectsEndpoints)
   │   • resolves the caller from the API key (ICurrentUserByApiKey)
   │   • sends start/finish progress messages (IMessagesDataManager → SignalR)
   ▼
MediatR command (e.g. CreateBackupRequestCommand)
   │   • validated by a FluentValidation validator
   ▼
Command handler (e.g. CreateBackupCommandHandler)
   │   • reads configuration via AppSettings
   │   • delegates the real work to ToolsManagement / DatabaseTools
   ▼
OneOf<TResult, Error[]>  →  Ok / BadRequest
```

This solution contains two local projects; everything else is referenced from sibling repositories
(see [Getting the source](#getting-the-source)).

| Project | Description |
|---------|-------------|
| `WebAgent` | The web host. Wires up Serilog, Swagger, API-key identity, SignalR, MediatR and the endpoint groups in `Program.cs`. |
| `LibDatabasesApi` | The database management API library — endpoints, MediatR commands, handlers, validators and mappers for all database operations. |

### API reference

All endpoints require authorization (a valid API key). The base path is `api/v1`.

#### Databases — `api/v1/databases`

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/checkrepairdatabase/{databaseName}` | Check and repair a database |
| POST | `/createbackup/{databaseName}/{dbServerFoldersSetName}` | Create a full backup of a database |
| POST | `/executecommand/{databaseName?}` | Execute an SQL command |
| GET  | `/getdatabasenames` | List databases on the server |
| GET  | `/getdatabasefolderssetnames` | List configured database folder-set names |
| GET  | `/getdatabaseconnectionnames` | List configured database connection names |
| GET  | `/isdatabaseexists/{databaseName}` | Check whether a database exists |
| PUT  | `/restorebackup/{databaseName}/{dbServerFoldersSetName}` | Restore a database from a backup |
| POST | `/recompileprocedures/{databaseName}` | Recompile stored procedures |
| GET  | `/testconnection/{databaseName?}` | Test the database server connection |
| POST | `/updatestatistics/{databaseName}` | Update database statistics |

#### Projects / services — `api/v1/projects`

| Method | Route | Description |
|--------|-------|-------------|
| GET    | `/getappsettingsversion/{serverSidePort}/{apiVersionId}` | Get a deployed app's settings version |
| GET    | `/getversion/{serverSidePort}/{apiVersionId}` | Get a deployed app's version |
| DELETE | `/removeprojectservice/{projectName}/{environmentName}/{isService}` | Remove a deployed project or service |
| POST   | `/startservice/{projectName}/{environmentName}` | Start a Windows service |
| POST   | `/stop/{projectName}/{environmentName}` | Stop a Windows service |
| POST   | `/update` | Deploy / update a project |
| POST   | `/updateservice` | Deploy / update a service |
| POST   | `/updatesettings` | Update a deployed app's settings |

> In `Development` the API surface is also browsable through Swagger UI (see below), and a set of
> test-tools endpoints and a SignalR messages hub are mapped as well.

### Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) or later
- A SQL Server instance the agent can connect to (for the database operations)
- Git (the solution is composed of several sibling repositories)

### Getting the source

WebAgent is built as part of a multi-repository solution. All repositories must be cloned **side by
side** under a common parent folder, because the solution (`WebAgent.slnx`) references them with
relative paths (`../ConnectionTools/...`, `../SystemTools/...`, etc.).

```sh
mkdir WebAgent
cd WebAgent
git clone git@github.com:merabza/ConnectionTools.git ConnectionTools
git clone git@github.com:merabza/SystemTools.git SystemTools
git clone git@github.com:merabza/WebAgent.git WebAgent
git clone git@github.com:merabza/WebSystemTools.git WebSystemTools
git clone git@github.com:merabza/WebAgentShared.git WebAgentShared
git clone git@github.com:merabza/WebAgentContracts.git WebAgentContracts
git clone git@github.com:merabza/ParametersManagement.git ParametersManagement
git clone git@github.com:merabza/ToolsManagement.git ToolsManagement
git clone git@github.com:merabza/DatabaseTools.git DatabaseTools
cd ..
```

Resulting folder layout:

```
WebAgent/                 ← parent folder
├── ConnectionTools/
├── DatabaseTools/
├── ParametersManagement/
├── SystemTools/
├── ToolsManagement/
├── WebAgent/             ← this repository (contains WebAgent.slnx)
├── WebAgentContracts/
├── WebAgentShared/
└── WebSystemTools/
```

### Build & run

From the `WebAgent` repository folder:

```sh
# Restore & build the whole solution
dotnet build WebAgent.slnx

# Run the web agent
dotnet run --project WebAgent/WebAgent.csproj
```

By default the agent listens on **http://*:5031** (configured via Kestrel in `appsettings.json`; the
launch profile uses `http://localhost:5031`). When running in the `Development` environment, the
browser opens Swagger UI at:

```
http://localhost:5031/swagger
```

You can also open `WebAgent.slnx` directly in Visual Studio or JetBrains Rider and run the `WebAgent`
project.

### Configuration

Configuration lives in `WebAgent/appsettings.json` (overridable per environment via
`appsettings.{Environment}.json`, user secrets and environment variables). Key sections:

| Section | Purpose |
|---------|---------|
| `Kestrel:Endpoints:Http:Url` | The address/port the agent listens on (default `http://*:5031`). |
| `Serilog` | Logging sinks — console and a daily rolling file. |
| `AppSettings:DatabaseServerData` | Default database connection, backup file-storage and smart-schema to use. |
| `AppSettings:DatabaseServerConnections` | Named SQL Server connections (server address, auth, backup parameters, database folder sets). |
| `AppSettings:DatabasesBackupFilesExchangeParameters` | How backup files are exchanged through file storages (local path, temp extensions, exchange storage / smart schema). |
| `AppSettings:SmartSchemas` | Backup retention rules (how many backups to keep per period). |
| `AppSettings:FileStorages` | File storage definitions used for backups and exchange (local paths, network shares, credentials). |
| `ApiKeys:AppSettingsByApiKey` | The accepted API keys and the remote IP address each key is allowed from. |
| `InstallerSettings` | Settings used when deploying / updating applications and services. |

> ⚠️ Do not commit real secrets (API keys, storage passwords, connection credentials) to source
> control. Use environment variables or [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)
> for sensitive values. The `appsettings.json` in the repo contains placeholder/sample values.

#### Authentication

Every API call must present a valid API key (see `WebSystemTools.ApiKeyIdentity`). Each key in
`ApiKeys:AppSettingsByApiKey` can be additionally restricted to a specific `RemoteIpAddress`. The
authenticated caller's name is then used to tag all SignalR progress messages for that operation.

### Running as a Windows Service

`Program.cs` calls `UseWindowsServiceOnWindows(...)`, so the same binary can be installed and run as
a Windows Service. Publish the project and register it with `sc.exe` (or your preferred tooling),
pointing the service at the published executable.

### Development notes

- Target framework: **net10.0**, `Nullable` enabled, `ImplicitUsings` disabled.
- The build treats warnings as errors and enforces code style (`TreatWarningsAsErrors`,
  `EnforceCodeStyleInBuild`, `AnalysisMode=All`), with **SonarAnalyzer.CSharp** running on every
  project — keep the build warning-free.
- NuGet package versions are managed centrally in `Directory.Packages.props`; shared build settings
  live in `Directory.Build.props`.
- `Documents/SqlSystemFolders.sql` is a helper script for discovering SQL Server's default data, log
  and backup directories.

### License

Released under the [MIT License](LICENSE). Copyright © 2023 Merab Zakalashvili.

---

<a name="ქართული"></a>

## ქართული

**WebAgent** არის მსუბუქი, დისტანციურად მართვადი აგენტი (ASP.NET Core Web API), რომელიც ეშვება
ბაზის / აპლიკაციის სერვერზე და აწვდის დაცულ HTTP API-ს ამ მანქანაზე **SQL ბაზებისა** და
**განთავსებული აპლიკაციების / Windows სერვისების** მართვისთვის.

ეს არის სერვერის მხარის კომპონენტი, რომელსაც დისტანციური მართვის ხელსაწყოები მიმართავენ ისეთი
ოპერაციების შესასრულებლად, როგორიცაა ბაზის ბექაფების შექმნა და აღდგენა, ბაზების შემოწმება და
შეკეთება, აპლიკაციების განთავსება და განახლება, სერვისების გაშვება/გაჩერება — ეს ყველაფერი
API-გასაღებით დაცული REST API-ის გავლით, SignalR-ით პროგრესის რეალურ დროში ანგარიშგებით.

### შესაძლებლობები

#### ბაზების მართვა (`api/v1/databases`)
- ბაზის სრული ბექაფების შექმნა და მათი გადატანა კონფიგურირებული ფაილსაცავის გავლით
- ბაზების აღდგენა ბექაფიდან
- ბაზების შემოწმება და შეკეთება
- სტატისტიკის განახლება და შენახული პროცედურების რეკომპილაცია
- ნებისმიერი SQL ბრძანების შესრულება
- ბაზების სახელების, კავშირების სახელებისა და საქაღალდეთა ნაკრების სახელების მიღება
- ბაზის სერვერთან კავშირის ტესტირება და ბაზის არსებობის შემოწმება

#### პროექტების / სერვისების მართვა (`api/v1/projects`)
- აპლიკაციებისა და Windows სერვისების განთავსება / განახლება
- Windows სერვისების გაშვება და გაჩერება
- განთავსებული პროექტის ან სერვისის წაშლა
- განთავსებული აპლიკაციის პარამეტრების განახლება
- განთავსებული აპლიკაციის მიმდინარე ვერსიისა და პარამეტრების ვერსიის მიღება

#### პლატფორმა
- API-გასაღებზე დაფუძნებული აუთენტიფიკაცია, თითოეული დაშორებული IP მისამართისთვის შეზღუდული
- ოპერაციის პროგრესის შეტყობინებები რეალურ დროში **SignalR**-ით
- **Swagger / OpenAPI** ინტერფეისი API-ის დასათვალიერებლად და გასატესტად
- **Serilog** სტრუქტურირებული ლოგირება (კონსოლი + დღიური rolling ფაილი)
- შესაძლებელია გაშვება როგორც კონსოლის აპლიკაცია, ისე **Windows სერვისად**

### ტექნოლოგიები

| სფერო | ტექნოლოგია |
|------|------------|
| Runtime | .NET 10.0 / ASP.NET Core (Minimal APIs) |
| Messaging / CQRS | [MediatR](https://github.com/jbogard/MediatR) (`ICommand` / `ICommandHandler`) |
| ვალიდაცია | [FluentValidation](https://fluentvalidation.net/) |
| შედეგების დამუშავება | [OneOf](https://github.com/mcintyre321/OneOf) discriminated union-ები (`OneOf<TResult, Error[]>`) |
| ლოგირება | [Serilog](https://serilog.net/) |
| რეალური დრო | ASP.NET Core SignalR |
| API დოკუმენტაცია | Swagger / OpenAPI |
| ბანერი | Figgle.Fonts (ASCII ბანერი გაშვებისას) |
| კოდის ხარისხი | Nullable ჩართული, `TreatWarningsAsErrors`, SonarAnalyzer.CSharp, ანალიზის სრული რეჟიმი |

### არქიტექტურა

აპლიკაცია მისდევს თხელ **endpoint → MediatR command → handler** მილსადენს:

```
HTTP მოთხოვნა
   │
   ▼
Minimal API endpoint (DatabasesEndpoints / ProjectsEndpoints)
   │   • ამოიცნობს გამომძახებელს API გასაღებიდან (ICurrentUserByApiKey)
   │   • აგზავნის დაწყება/დასრულების პროგრესის შეტყობინებებს (IMessagesDataManager → SignalR)
   ▼
MediatR command (მაგ. CreateBackupRequestCommand)
   │   • მოწმდება FluentValidation ვალიდატორით
   ▼
Command handler (მაგ. CreateBackupCommandHandler)
   │   • კითხულობს კონფიგურაციას AppSettings-ის გავლით
   │   • რეალურ სამუშაოს გადასცემს ToolsManagement / DatabaseTools-ს
   ▼
OneOf<TResult, Error[]>  →  Ok / BadRequest
```

ეს solution შეიცავს ორ ლოკალურ პროექტს; დანარჩენი ყველაფერი მოთავსებულია გვერდით განთავსებულ
რეპოზიტორიებში (იხ. [წყაროს მიღება](#წყაროს-მიღება)).

| პროექტი | აღწერა |
|---------|--------|
| `WebAgent` | ვებ-ჰოსტი. `Program.cs`-ში აერთიანებს Serilog-ს, Swagger-ს, API-გასაღების იდენტობას, SignalR-ს, MediatR-ს და endpoint-ების ჯგუფებს. |
| `LibDatabasesApi` | ბაზების მართვის API ბიბლიოთეკა — endpoint-ები, MediatR command-ები, handler-ები, ვალიდატორები და mapper-ები ბაზის ყველა ოპერაციისთვის. |

### API ცნობარი

ყველა endpoint მოითხოვს ავტორიზაციას (ვალიდურ API გასაღებს). საბაზისო გზაა `api/v1`.

#### ბაზები — `api/v1/databases`

| მეთოდი | მარშრუტი | აღწერა |
|--------|----------|--------|
| POST | `/checkrepairdatabase/{databaseName}` | ბაზის შემოწმება და შეკეთება |
| POST | `/createbackup/{databaseName}/{dbServerFoldersSetName}` | ბაზის სრული ბექაფის შექმნა |
| POST | `/executecommand/{databaseName?}` | SQL ბრძანების შესრულება |
| GET  | `/getdatabasenames` | სერვერზე არსებული ბაზების სია |
| GET  | `/getdatabasefolderssetnames` | კონფიგურირებული საქაღალდეთა ნაკრების სახელების სია |
| GET  | `/getdatabaseconnectionnames` | კონფიგურირებული კავშირების სახელების სია |
| GET  | `/isdatabaseexists/{databaseName}` | ბაზის არსებობის შემოწმება |
| PUT  | `/restorebackup/{databaseName}/{dbServerFoldersSetName}` | ბაზის აღდგენა ბექაფიდან |
| POST | `/recompileprocedures/{databaseName}` | შენახული პროცედურების რეკომპილაცია |
| GET  | `/testconnection/{databaseName?}` | ბაზის სერვერთან კავშირის ტესტირება |
| POST | `/updatestatistics/{databaseName}` | ბაზის სტატისტიკის განახლება |

#### პროექტები / სერვისები — `api/v1/projects`

| მეთოდი | მარშრუტი | აღწერა |
|--------|----------|--------|
| GET    | `/getappsettingsversion/{serverSidePort}/{apiVersionId}` | განთავსებული აპლიკაციის პარამეტრების ვერსიის მიღება |
| GET    | `/getversion/{serverSidePort}/{apiVersionId}` | განთავსებული აპლიკაციის ვერსიის მიღება |
| DELETE | `/removeprojectservice/{projectName}/{environmentName}/{isService}` | განთავსებული პროექტის ან სერვისის წაშლა |
| POST   | `/startservice/{projectName}/{environmentName}` | Windows სერვისის გაშვება |
| POST   | `/stop/{projectName}/{environmentName}` | Windows სერვისის გაჩერება |
| POST   | `/update` | პროექტის განთავსება / განახლება |
| POST   | `/updateservice` | სერვისის განთავსება / განახლება |
| POST   | `/updatesettings` | განთავსებული აპლიკაციის პარამეტრების განახლება |

> `Development` რეჟიმში API-ის დათვალიერება ასევე შესაძლებელია Swagger UI-ით (იხ. ქვემოთ), და
> დამატებით განთავსებულია test-tools endpoint-ები და SignalR შეტყობინებების hub.

### წინაპირობები

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) ან უფრო ახალი
- SQL Server-ის ინსტანცია, რომელთანაც აგენტი დაუკავშირდება (ბაზის ოპერაციებისთვის)
- Git (solution რამდენიმე გვერდითი რეპოზიტორიისგან შედგება)

### წყაროს მიღება

WebAgent იგება მრავალ-რეპოზიტორიული solution-ის ნაწილად. ყველა რეპოზიტორია უნდა დაიკლონოს
**გვერდიგვერდ**, საერთო მშობელ საქაღალდეში, რადგან solution (`WebAgent.slnx`) მათ მიმართავს
ფარდობითი გზებით (`../ConnectionTools/...`, `../SystemTools/...` და ა.შ.).

```sh
mkdir WebAgent
cd WebAgent
git clone git@github.com:merabza/ConnectionTools.git ConnectionTools
git clone git@github.com:merabza/SystemTools.git SystemTools
git clone git@github.com:merabza/WebAgent.git WebAgent
git clone git@github.com:merabza/WebSystemTools.git WebSystemTools
git clone git@github.com:merabza/WebAgentShared.git WebAgentShared
git clone git@github.com:merabza/WebAgentContracts.git WebAgentContracts
git clone git@github.com:merabza/ParametersManagement.git ParametersManagement
git clone git@github.com:merabza/ToolsManagement.git ToolsManagement
git clone git@github.com:merabza/DatabaseTools.git DatabaseTools
cd ..
```

შედეგად მიღებული საქაღალდის სტრუქტურა:

```
WebAgent/                 ← მშობელი საქაღალდე
├── ConnectionTools/
├── DatabaseTools/
├── ParametersManagement/
├── SystemTools/
├── ToolsManagement/
├── WebAgent/             ← ეს რეპოზიტორია (შეიცავს WebAgent.slnx-ს)
├── WebAgentContracts/
├── WebAgentShared/
└── WebSystemTools/
```

### აგება და გაშვება

`WebAgent` რეპოზიტორიის საქაღალდიდან:

```sh
# მთლიანი solution-ის აღდგენა და აგება
dotnet build WebAgent.slnx

# ვებ-აგენტის გაშვება
dotnet run --project WebAgent/WebAgent.csproj
```

ნაგულისხმევად აგენტი უსმენს მისამართზე **http://*:5031** (კონფიგურირდება Kestrel-ით
`appsettings.json`-ში; გაშვების პროფილი იყენებს `http://localhost:5031`-ს). `Development` გარემოში
გაშვებისას ბრაუზერი იხსნება Swagger UI-ზე:

```
http://localhost:5031/swagger
```

ასევე შეგიძლია `WebAgent.slnx` პირდაპირ გახსნა Visual Studio-ში ან JetBrains Rider-ში და გაუშვა
`WebAgent` პროექტი.

### კონფიგურაცია

კონფიგურაცია მოთავსებულია `WebAgent/appsettings.json`-ში (გადაფარვადია გარემოს მიხედვით
`appsettings.{Environment}.json`-ით, user secrets-ით და გარემოს ცვლადებით). ძირითადი სექციები:

| სექცია | დანიშნულება |
|--------|-------------|
| `Kestrel:Endpoints:Http:Url` | მისამართი/პორტი, რომელსაც აგენტი უსმენს (ნაგულისხმევი `http://*:5031`). |
| `Serilog` | ლოგირების მიმღებები — კონსოლი და დღიური rolling ფაილი. |
| `AppSettings:DatabaseServerData` | ნაგულისხმევი ბაზის კავშირი, ბექაფის ფაილსაცავი და smart-schema. |
| `AppSettings:DatabaseServerConnections` | დასახელებული SQL Server კავშირები (სერვერის მისამართი, ავტორიზაცია, ბექაფის პარამეტრები, ბაზის საქაღალდეთა ნაკრები). |
| `AppSettings:DatabasesBackupFilesExchangeParameters` | როგორ ხდება ბექაფის ფაილების გაცვლა ფაილსაცავების გავლით (ლოკალური გზა, დროებითი გაფართოებები, exchange საცავი / smart schema). |
| `AppSettings:SmartSchemas` | ბექაფების შენახვის წესები (რამდენი ბექაფი შენარჩუნდეს თითო პერიოდზე). |
| `AppSettings:FileStorages` | ფაილსაცავების განსაზღვრებები ბექაფისა და გაცვლისთვის (ლოკალური გზები, ქსელური share-ები, რწმუნებები). |
| `ApiKeys:AppSettingsByApiKey` | მიღებული API გასაღებები და დაშორებული IP მისამართი, საიდანაც თითოეული გასაღები დაიშვება. |
| `InstallerSettings` | აპლიკაციებისა და სერვისების განთავსების / განახლების პარამეტრები. |

> ⚠️ არ ჩააქციოთ რეალური საიდუმლოები (API გასაღებები, საცავის პაროლები, კავშირის რწმუნებები)
> version control-ში. სენსიტიური მნიშვნელობებისთვის გამოიყენეთ გარემოს ცვლადები ან
> [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets). რეპოზიტორიაში
> არსებული `appsettings.json` შეიცავს მხოლოდ ნიმუშ/placeholder მნიშვნელობებს.

#### აუთენტიფიკაცია

ყოველი API გამოძახება უნდა წარადგენდეს ვალიდურ API გასაღებს (იხ. `WebSystemTools.ApiKeyIdentity`).
`ApiKeys:AppSettingsByApiKey`-ში თითოეული გასაღები დამატებით შეიძლება შეიზღუდოს კონკრეტული
`RemoteIpAddress`-ით. აუთენტიფიცირებული გამომძახებლის სახელი შემდეგ გამოიყენება ამ ოპერაციის ყველა
SignalR პროგრესის შეტყობინების მოსანიშნად.

### Windows სერვისად გაშვება

`Program.cs` იძახებს `UseWindowsServiceOnWindows(...)`-ს, ამიტომ იგივე ბინარი შეიძლება დაინსტალირდეს
და გაეშვას Windows სერვისად. გამოაქვეყნე (publish) პროექტი და დაარეგისტრირე `sc.exe`-ით (ან შენთვის
სასურველი ხელსაწყოთი), სერვისი მიმართე გამოქვეყნებულ შესრულებად ფაილზე.

### დეველოპერული შენიშვნები

- სამიზნე ფრეიმვორქი: **net10.0**, `Nullable` ჩართული, `ImplicitUsings` გამორთული.
- აგება გაფრთხილებებს ეპყრობა შეცდომებად და აიძულებს კოდის სტილს (`TreatWarningsAsErrors`,
  `EnforceCodeStyleInBuild`, `AnalysisMode=All`), **SonarAnalyzer.CSharp**-ით ყველა პროექტზე —
  შეინარჩუნე აგება გაფრთხილებების გარეშე.
- NuGet პაკეტების ვერსიები ცენტრალურად იმართება `Directory.Packages.props`-ში; საერთო აგების
  პარამეტრები მოთავსებულია `Directory.Build.props`-ში.
- `Documents/SqlSystemFolders.sql` დამხმარე სკრიპტია SQL Server-ის ნაგულისხმევი data, log და backup
  საქაღალდეების აღმოსაჩენად.

### ლიცენზია

გამოშვებულია [MIT ლიცენზიით](LICENSE). Copyright © 2023 Merab Zakalashvili.
