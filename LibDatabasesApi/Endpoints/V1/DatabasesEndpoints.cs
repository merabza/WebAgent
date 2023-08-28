using System.Diagnostics;
using System.Threading.Tasks;
using ApiToolsShared;
using LibDatabasesApi.CommandRequests;
using LibDatabasesApi.Handlers;
using LibDatabasesApi.Mappers;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAgentDatabasesApiContracts.V1.Requests;
using WebAgentDatabasesApiContracts.V1.Routes;
using WebAgentMessagesContracts;
using WebAgentProjectsApiContracts.V1.Requests;
using WebInstallers;

namespace LibDatabasesApi.Endpoints.V1;

// ReSharper disable once UnusedType.Global
public sealed class DatabasesEndpoints : IInstaller
{
    public int InstallPriority => 50;
    public int ServiceUsePriority => 50;

    public void InstallServices(WebApplicationBuilder builder, string[] args)
    {
    }

    public void UseServices(WebApplication app)
    {
        var group = app.MapGroup(DatabaseApiRoutes.Database.DatabaseBase).RequireAuthorization();

        group.MapPost(DatabaseApiRoutes.Database.CheckRepairDatabase, CheckRepairDatabase);
        group.MapPost(DatabaseApiRoutes.Database.CreateBackup, CreateBackup);
        group.MapPost(DatabaseApiRoutes.Database.ExecuteCommand, ExecuteCommand);
        group.MapGet(DatabaseApiRoutes.Database.GetDatabaseNames, GetDatabaseNames);
        group.MapGet(DatabaseApiRoutes.Database.IsDatabaseExists, IsDatabaseExists);
        group.MapPut(DatabaseApiRoutes.Database.RestoreBackup, RestoreBackup);
        group.MapPost(DatabaseApiRoutes.Database.RecompileProcedures, RecompileProcedures);
        group.MapGet(DatabaseApiRoutes.Database.TestConnection, TestConnection);
        group.MapPost(DatabaseApiRoutes.Database.UpdateStatistics, UpdateStatistics);
    }

    // POST api/database/checkrepairdatabase/{databaseName}
    private static async Task<IResult> CheckRepairDatabase([FromRoute] string databaseName, HttpRequest httpRequest,
        IMediator mediator, IMessagesDataManager messagesDataManager)
    {
        var userName = httpRequest.HttpContext.User.Identity?.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(CheckRepairDatabase)} started");
        Debug.WriteLine($"Call {nameof(CheckRepairDatabaseCommandHandler)} from {nameof(CheckRepairDatabase)}");

        var command = CheckRepairDatabaseCommandRequest.Create(databaseName, userName);
        var result = await mediator.Send(command);

        await messagesDataManager.SendMessage(userName, $"{nameof(CheckRepairDatabase)} finished");
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }

    // POST api/database/createbackup/{databaseName}
    private static async Task<IResult> CreateBackup([FromRoute] string databaseName, HttpRequest httpRequest,
        [FromBody] CreateBackupRequest? request, IMediator mediator, IMessagesDataManager messagesDataManager)
    {
        var userName = httpRequest.HttpContext.User.Identity?.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(CreateBackup)} started");
        Debug.WriteLine($"Call {nameof(CreateBackupCommandHandler)} from {nameof(CreateBackup)}");

        if (request is null)
            return Results.BadRequest(ApiErrors.RequestIsEmpty);
        var command = request.AdaptTo(databaseName, userName);
        var result = await mediator.Send(command);

        await messagesDataManager.SendMessage(userName, $"{nameof(CreateBackup)} finished");
        return result.Match(Results.Ok, Results.BadRequest);
    }

    // POST api/database/executecommand/{databaseName}
    private static async Task<IResult> ExecuteCommand([FromRoute] string databaseName, HttpRequest httpRequest,
        [FromBody] string? commandText, IMediator mediator, IMessagesDataManager messagesDataManager)
    {
        var userName = httpRequest.HttpContext.User.Identity?.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(ExecuteCommand)} started");
        Debug.WriteLine($"Call {nameof(ExecuteCommandCommandHandler)} from {nameof(ExecuteCommand)}");

        //ExecuteCommandCommandRequest
        var command = ExecuteCommandCommandRequest.Create(databaseName, commandText, userName);
        var result = await mediator.Send(command);

        await messagesDataManager.SendMessage(userName, $"{nameof(ExecuteCommand)} finished");
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }

    // GET api/database/getdatabasenames
    private static async Task<IResult> GetDatabaseNames(HttpRequest httpRequest, IMediator mediator,
        IMessagesDataManager messagesDataManager)
    {
        var userName = httpRequest.HttpContext.User.Identity?.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(GetDatabaseNames)} started");
        Debug.WriteLine($"Call {nameof(GetDatabaseNamesCommandHandler)} from {nameof(GetDatabaseNames)}");

        //GetDatabaseNamesCommandRequest
        var command = GetDatabaseNamesCommandRequest.Create(userName);
        var result = await mediator.Send(command);

        await messagesDataManager.SendMessage(userName, $"{nameof(GetDatabaseNames)} finished");
        return result.Match(Results.Ok, Results.BadRequest);
    }

    // GET api/database/isdatabaseexists/{databaseName}
    private static async Task<IResult> IsDatabaseExists([FromRoute] string databaseName, HttpRequest httpRequest,
        IMediator mediator, IMessagesDataManager messagesDataManager)
    {
        var userName = httpRequest.HttpContext.User.Identity?.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(IsDatabaseExists)} started");
        Debug.WriteLine($"Call {nameof(IsDatabaseExistsCommandHandler)} from {nameof(IsDatabaseExists)}");

        //IsDatabaseExistsCommandRequest

        var command = IsDatabaseExistsCommandRequest.Create(databaseName, userName);
        var result = await mediator.Send(command);

        await messagesDataManager.SendMessage(userName, $"{nameof(IsDatabaseExists)} finished");
        return result.Match(Results.Ok, Results.BadRequest);
    }

    // POST api/database/recompileprocedures/{databaseName}
    private static async Task<IResult> RecompileProcedures([FromRoute] string databaseName, HttpRequest httpRequest,
        IMediator mediator, IMessagesDataManager messagesDataManager)
    {
        var userName = httpRequest.HttpContext.User.Identity?.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(RecompileProcedures)} started");
        Debug.WriteLine($"Call {nameof(RecompileProceduresCommandHandler)} from {nameof(RecompileProcedures)}");

        //RecompileProceduresCommandRequest

        var command = RecompileProceduresCommandRequest.Create(databaseName, userName);
        var result = await mediator.Send(command);

        await messagesDataManager.SendMessage(userName, $"{nameof(RecompileProcedures)} finished");
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }

    // PUT restorebackup/{databaseName}
    private static async Task<IResult> RestoreBackup([FromRoute] string databaseName, HttpRequest httpRequest,
        [FromBody] RestoreBackupRequest? request, IMediator mediator, IMessagesDataManager messagesDataManager)
    {
        var userName = httpRequest.HttpContext.User.Identity?.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(RestoreBackup)} started");
        Debug.WriteLine($"Call {nameof(RestoreBackupCommandHandler)} from {nameof(RestoreBackup)}");

        if (request is null)
            return Results.BadRequest(ApiErrors.RequestIsEmpty);
        var command = request.AdaptTo(databaseName, userName);
        var result = await mediator.Send(command);

        await messagesDataManager.SendMessage(userName, $"{nameof(RestoreBackup)} finished");
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }

    // GET api/database/testconnection/{databaseName?}
    private static async Task<IResult> TestConnection([FromRoute] string? databaseName, HttpRequest httpRequest,
        IMediator mediator, IMessagesDataManager messagesDataManager)
    {
        var userName = httpRequest.HttpContext.User.Identity?.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(TestConnection)} started");
        Debug.WriteLine($"Call {nameof(TestConnectionCommandHandler)} from {nameof(TestConnection)}");

        //TestConnectionCommandRequest

        var command = TestConnectionCommandRequest.Create(databaseName, userName);
        var result = await mediator.Send(command);

        await messagesDataManager.SendMessage(userName, $"{nameof(TestConnection)} finished");
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }

    // POST api/database/updatestatistics/{databaseName}
    private static async Task<IResult> UpdateStatistics([FromRoute] string databaseName, HttpRequest httpRequest,
        IMediator mediator, IMessagesDataManager messagesDataManager)
    {
        var userName = httpRequest.HttpContext.User.Identity?.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(UpdateStatistics)} started");
        Debug.WriteLine($"Call {nameof(UpdateStatisticsCommandHandler)} from {nameof(UpdateStatistics)}");

        //UpdateStatisticsCommandRequest

        var command = UpdateStatisticsCommandRequest.Create(databaseName, userName);
        var result = await mediator.Send(command);

        await messagesDataManager.SendMessage(userName, $"{nameof(UpdateStatistics)} finished");
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }
}