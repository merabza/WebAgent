using System.Threading.Tasks;
using ApiToolsShared;
using LibDatabasesApi.CommandRequests;
using LibDatabasesApi.Mappers;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebAgentContracts.V1.Requests;
using WebAgentDbContracts.V1.Requests;
using WebAgentDbContracts.V1.Routes;
using WebInstallers;

namespace LibDatabasesApi.Endpoints.V1;

public sealed class DatabasesEndpoints : IInstaller
{
    public int InstallPriority => 50;
    public int ServiceUsePriority => 50;

    public void InstallServices(WebApplicationBuilder builder, string[] args)
    {
    }

    public void UseServices(WebApplication app)
    {
        app.MapPost(DatabaseApiRoutes.Database.CheckRepairDatabase, CheckRepairDatabase)
            .AddEndpointFilter<ApiKeysChecker>();
        app.MapPost(DatabaseApiRoutes.Database.CreateBackup, CreateBackup)
            .AddEndpointFilter<ApiKeysChecker>();
        app.MapPost(DatabaseApiRoutes.Database.ExecuteCommand, ExecuteCommand).AddEndpointFilter<ApiKeysChecker>();
        app.MapGet(DatabaseApiRoutes.Database.GetDatabaseNames, GetDatabaseNames).AddEndpointFilter<ApiKeysChecker>();
        app.MapGet(DatabaseApiRoutes.Database.IsDatabaseExists, IsDatabaseExists).AddEndpointFilter<ApiKeysChecker>();
        app.MapPut(DatabaseApiRoutes.Database.RestoreBackup, RestoreBackup).AddEndpointFilter<ApiKeysChecker>();
        app.MapPost(DatabaseApiRoutes.Database.RecompileProcedures, RecompileProcedures)
            .AddEndpointFilter<ApiKeysChecker>();
        app.MapGet(DatabaseApiRoutes.Database.TestConnection, TestConnection).AddEndpointFilter<ApiKeysChecker>();
        app.MapPost(DatabaseApiRoutes.Database.UpdateStatistics, UpdateStatistics).AddEndpointFilter<ApiKeysChecker>();
    }

    private static async Task<IResult> CheckRepairDatabase(ILogger<DatabasesEndpoints> logger, IConfiguration config,
        string databaseName,
        [FromQuery] string? apiKey, HttpRequest httpRequest, IMediator mediator)
    {
        var command = CheckRepairDatabaseCommandRequest.Create(databaseName);
        var result = await mediator.Send(command);
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }

    //// POST api/database/createbackup
    //[HttpPost(ApiRoutes.Database.CreateBackup)]
    private static async Task<IResult> CreateBackup(ILogger<DatabasesEndpoints> logger, IConfiguration config,
        string databaseName,
        [FromQuery] string apiKey, HttpRequest httpRequest, [FromBody] CreateBackupRequest? request, IMediator mediator)
    {
        if (request is null)
            return Results.BadRequest(ApiErrors.RequestIsEmpty);
        var command = request.AdaptTo(databaseName);
        var result = await mediator.Send(command);
        return result.Match(Results.Ok, Results.BadRequest);
    }

    //// POST api/database/executecommand
    //[HttpPost(ApiRoutes.Database.ExecuteCommand)]
    private static async Task<IResult> ExecuteCommand(ILogger<DatabasesEndpoints> logger, IConfiguration config,
        string databaseName,
        [FromQuery] string apiKey, HttpRequest httpRequest, [FromBody] string? commandText, IMediator mediator)
    {
        //ExecuteCommandCommandRequest
        var command = ExecuteCommandCommandRequest.Create(databaseName, commandText);
        var result = await mediator.Send(command);
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }

    //// GET api/database/getdatabasenames
    //[HttpGet(ApiRoutes.Database.GetDatabaseNames)]
    private static async Task<IResult> GetDatabaseNames(ILogger<DatabasesEndpoints> logger, IConfiguration config,
        [FromQuery] string apiKey, HttpRequest httpRequest, IMediator mediator)
    {
        //GetDatabaseNamesCommandRequest
        var command = GetDatabaseNamesCommandRequest.Create();
        var result = await mediator.Send(command);
        return result.Match(Results.Ok, Results.BadRequest);
    }

    //// GET api/database/isdatabaseexists
    //[HttpGet(ApiRoutes.Database.IsDatabaseExists)]
    private static async Task<IResult> IsDatabaseExists(ILogger<DatabasesEndpoints> logger, IConfiguration config,
        string databaseName, [FromQuery] string apiKey, HttpRequest httpRequest, IMediator mediator)
    {
        //IsDatabaseExistsCommandRequest

        var command = IsDatabaseExistsCommandRequest.Create(databaseName);
        var result = await mediator.Send(command);
        return result.Match(Results.Ok, Results.BadRequest);
    }

    //[HttpPut(ApiRoutes.Database.RestoreBackup)]
    private static async Task<IResult> RestoreBackup(ILogger<DatabasesEndpoints> logger, IConfiguration config,
        string databaseName,
        [FromQuery] string apiKey, HttpRequest httpRequest, [FromBody] RestoreBackupRequest? request,
        IMediator mediator)
    {
        if (request is null)
            return Results.BadRequest(ApiErrors.RequestIsEmpty);
        var command = request.AdaptTo(databaseName);
        var result = await mediator.Send(command);
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }


    //// POST api/database/recompileprocedures
    //[HttpPost(ApiRoutes.Database.RecompileProcedures)]
    private static async Task<IResult> RecompileProcedures(ILogger<DatabasesEndpoints> logger, IConfiguration config,
        string databaseName,
        [FromQuery] string apiKey, HttpRequest httpRequest, IMediator mediator)
    {
        //RecompileProceduresCommandRequest

        var command = RecompileProceduresCommandRequest.Create(databaseName);
        var result = await mediator.Send(command);
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }


    //// GET api/database/testconnection
    //[HttpGet(ApiRoutes.Database.TestConnection)]
    private static async Task<IResult> TestConnection(ILogger<DatabasesEndpoints> logger, IConfiguration config,
        string? databaseName,
        [FromQuery] string apiKey, HttpRequest httpRequest, IMediator mediator)
    {
        //TestConnectionCommandRequest

        var command = TestConnectionCommandRequest.Create(databaseName);
        var result = await mediator.Send(command);
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }


    //// POST api/database/updatestatistics
    //[HttpPost(ApiRoutes.Database.UpdateStatistics)]
    private static async Task<IResult> UpdateStatistics(ILogger<DatabasesEndpoints> logger, IConfiguration config,
        string databaseName,
        [FromQuery] string apiKey, HttpRequest httpRequest, IMediator mediator)
    {
        //UpdateStatisticsCommandRequest

        var command = UpdateStatisticsCommandRequest.Create(databaseName);
        var result = await mediator.Send(command);
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }
}