using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ApiContracts.Errors;
using ApiKeyIdentity;
using LibDatabaseParameters;
using LibDatabasesApi.CommandRequests;
using LibDatabasesApi.Handlers;
using LibDatabasesApi.Mappers;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemToolsShared;
using WebAgentDatabasesApiContracts.V1.Requests;
using WebAgentDatabasesApiContracts.V1.Routes;
using WebInstallers;

namespace LibDatabasesApi.Endpoints.V1;

// ReSharper disable once UnusedType.Global
public sealed class DatabasesEndpoints : IInstaller
{
    public int InstallPriority => 50;
    public int ServiceUsePriority => 50;

    public bool InstallServices(WebApplicationBuilder builder, bool debugMode, string[] args,
        Dictionary<string, string> parameters)
    {
        return true;
    }

    public bool UseServices(WebApplication app, bool debugMode)
    {
        if (debugMode)
            Console.WriteLine($"{GetType().Name}.{nameof(UseServices)} Started");

        var group = app.MapGroup(DatabaseApiRoutes.ApiBase + DatabaseApiRoutes.Database.DatabaseBase)
            .RequireAuthorization();

        group.MapPost(DatabaseApiRoutes.Database.CheckRepairDatabase, CheckRepairDatabase);
        group.MapPost(DatabaseApiRoutes.Database.CreateBackup, CreateBackup);
        group.MapPost(DatabaseApiRoutes.Database.ExecuteCommand, ExecuteCommand);
        group.MapGet(DatabaseApiRoutes.Database.GetDatabaseNames, GetDatabaseNames);
        group.MapGet(DatabaseApiRoutes.Database.GetDatabaseFoldersSetNames, GetDatabaseFoldersSetNames);
        group.MapGet(DatabaseApiRoutes.Database.GetDatabaseConnectionNames, GetDatabaseConnectionNames);
        group.MapGet(DatabaseApiRoutes.Database.IsDatabaseExists, IsDatabaseExists);
        group.MapPut(DatabaseApiRoutes.Database.RestoreBackup, RestoreBackup);
        group.MapPost(DatabaseApiRoutes.Database.RecompileProcedures, RecompileProcedures);
        group.MapGet(DatabaseApiRoutes.Database.TestConnection, TestConnection);
        group.MapPost(DatabaseApiRoutes.Database.UpdateStatistics, UpdateStatistics);

        if (debugMode)
            Console.WriteLine($"{GetType().Name}.{nameof(UseServices)} Finished");

        return true;
    }

    // POST api/database/checkrepairdatabase/{databaseName}
    private static async Task<IResult> CheckRepairDatabase([FromRoute] string databaseName,
        ICurrentUserByApiKey currentUserByApiKey, IMediator mediator, IMessagesDataManager messagesDataManager,
        CancellationToken cancellationToken = default)
    {
        var userName = currentUserByApiKey.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(CheckRepairDatabase)} started", cancellationToken);
        Debug.WriteLine($"Call {nameof(CheckRepairDatabaseCommandHandler)} from {nameof(CheckRepairDatabase)}");

        var command = CheckRepairDatabaseCommandRequest.Create(databaseName, userName);
        var result = await mediator.Send(command, cancellationToken);

        await messagesDataManager.SendMessage(userName, $"{nameof(CheckRepairDatabase)} finished", cancellationToken);
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }

    // POST api/database/createbackup/{databaseName}/{dbServerFoldersSetName}
    private static async Task<IResult> CreateBackup([FromRoute] string databaseName,
        [FromRoute] string dbServerFoldersSetName, [FromBody] DatabaseBackupParametersDomain? dbBackupParameters,
        ICurrentUserByApiKey currentUserByApiKey, IMediator mediator, IMessagesDataManager messagesDataManager,
        CancellationToken cancellationToken = default)
    {
        var userName = currentUserByApiKey.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(CreateBackup)} started", cancellationToken);
        Debug.WriteLine($"Call {nameof(CreateBackupCommandHandler)} from {nameof(CreateBackup)}");

        var command =
            new CreateBackupCommandRequest(databaseName, dbServerFoldersSetName, dbBackupParameters, userName);
        var result = await mediator.Send(command, cancellationToken);

        await messagesDataManager.SendMessage(userName, $"{nameof(CreateBackup)} finished", cancellationToken);
        return result.Match(Results.Ok, Results.BadRequest);
    }

    // POST api/database/executecommand/{databaseName}
    private static async Task<IResult> ExecuteCommand([FromRoute] string databaseName,
        ICurrentUserByApiKey currentUserByApiKey, [FromBody] string? commandText, IMediator mediator,
        IMessagesDataManager messagesDataManager, CancellationToken cancellationToken = default)
    {
        var userName = currentUserByApiKey.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(ExecuteCommand)} started", cancellationToken);
        Debug.WriteLine($"Call {nameof(ExecuteCommandCommandHandler)} from {nameof(ExecuteCommand)}");

        //ExecuteCommandCommandRequest
        var command = ExecuteCommandCommandRequest.Create(databaseName, commandText, userName);
        var result = await mediator.Send(command, cancellationToken);

        await messagesDataManager.SendMessage(userName, $"{nameof(ExecuteCommand)} finished", cancellationToken);
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }

    // GET api/database/getdatabaseconnectionnames
    private static async Task<IResult> GetDatabaseConnectionNames(ICurrentUserByApiKey currentUserByApiKey,
        IMediator mediator, IMessagesDataManager messagesDataManager, CancellationToken cancellationToken = default)
    {
        var userName = currentUserByApiKey.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(GetDatabaseConnectionNames)} started",
            cancellationToken);
        Debug.WriteLine(
            $"Call {nameof(GetDatabaseConnectionNamesCommandHandler)} from {nameof(GetDatabaseConnectionNames)}");

        //GetDatabaseConnectionNamesCommandRequest
        var command = GetDatabaseConnectionNamesCommandRequest.Create(userName);
        var result = await mediator.Send(command, cancellationToken);

        await messagesDataManager.SendMessage(userName, $"{nameof(GetDatabaseConnectionNames)} finished",
            cancellationToken);
        return result.Match(Results.Ok, Results.BadRequest);
    }

    // GET api/database/getdatabasefolderssetnames
    private static async Task<IResult> GetDatabaseFoldersSetNames(ICurrentUserByApiKey currentUserByApiKey,
        IMediator mediator, IMessagesDataManager messagesDataManager, CancellationToken cancellationToken = default)
    {
        var userName = currentUserByApiKey.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(GetDatabaseFoldersSetNames)} started",
            cancellationToken);
        Debug.WriteLine(
            $"Call {nameof(GetDatabaseFoldersSetNamesCommandHandler)} from {nameof(GetDatabaseFoldersSetNames)}");

        //GetDatabaseFoldersSetsCommandRequest
        var command = GetDatabaseFoldersSetNamesCommandRequest.Create(userName);
        var result = await mediator.Send(command, cancellationToken);

        await messagesDataManager.SendMessage(userName, $"{nameof(GetDatabaseFoldersSetNames)} finished",
            cancellationToken);
        return result.Match(Results.Ok, Results.BadRequest);
    }

    // GET api/database/getdatabasenames
    private static async Task<IResult> GetDatabaseNames(ICurrentUserByApiKey currentUserByApiKey, IMediator mediator,
        IMessagesDataManager messagesDataManager, CancellationToken cancellationToken = default)
    {
        var userName = currentUserByApiKey.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(GetDatabaseNames)} started", cancellationToken);
        Debug.WriteLine($"Call {nameof(GetDatabaseNamesCommandHandler)} from {nameof(GetDatabaseNames)}");

        //GetDatabaseNamesCommandRequest
        var command = GetDatabaseNamesCommandRequest.Create(userName);
        var result = await mediator.Send(command, cancellationToken);

        await messagesDataManager.SendMessage(userName, $"{nameof(GetDatabaseNames)} finished", cancellationToken);
        return result.Match(Results.Ok, Results.BadRequest);
    }

    // GET api/database/isdatabaseexists/{databaseName}
    private static async Task<IResult> IsDatabaseExists([FromRoute] string databaseName,
        ICurrentUserByApiKey currentUserByApiKey, IMediator mediator, IMessagesDataManager messagesDataManager,
        CancellationToken cancellationToken = default)
    {
        var userName = currentUserByApiKey.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(IsDatabaseExists)} started", cancellationToken);
        Debug.WriteLine($"Call {nameof(IsDatabaseExistsCommandHandler)} from {nameof(IsDatabaseExists)}");

        //IsDatabaseExistsCommandRequest

        var command = IsDatabaseExistsCommandRequest.Create(databaseName, userName);
        var result = await mediator.Send(command, cancellationToken);

        await messagesDataManager.SendMessage(userName, $"{nameof(IsDatabaseExists)} finished", cancellationToken);
        return result.Match(Results.Ok, Results.BadRequest);
    }

    // POST api/database/recompileprocedures/{databaseName}
    private static async Task<IResult> RecompileProcedures([FromRoute] string databaseName,
        ICurrentUserByApiKey currentUserByApiKey, IMediator mediator, IMessagesDataManager messagesDataManager,
        CancellationToken cancellationToken = default)
    {
        var userName = currentUserByApiKey.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(RecompileProcedures)} started", cancellationToken);
        Debug.WriteLine($"Call {nameof(RecompileProceduresCommandHandler)} from {nameof(RecompileProcedures)}");

        //RecompileProceduresCommandRequest

        var command = RecompileProceduresCommandRequest.Create(databaseName, userName);
        var result = await mediator.Send(command, cancellationToken);

        await messagesDataManager.SendMessage(userName, $"{nameof(RecompileProcedures)} finished", cancellationToken);
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }

    // PUT restorebackup/{databaseName}/{dbServerFoldersSetName}
    private static async Task<IResult> RestoreBackup([FromRoute] string databaseName,
        [FromRoute] string dbServerFoldersSetName, ICurrentUserByApiKey currentUserByApiKey,
        [FromBody] RestoreBackupRequest? request, IMediator mediator, IMessagesDataManager messagesDataManager,
        CancellationToken cancellationToken = default)
    {
        var userName = currentUserByApiKey.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(RestoreBackup)} started", cancellationToken);
        Debug.WriteLine($"Call {nameof(RestoreBackupCommandHandler)} from {nameof(RestoreBackup)}");

        if (request is null)
            return Results.BadRequest(new[] { ApiErrors.RequestIsEmpty });
        var command = request.AdaptTo(databaseName, dbServerFoldersSetName, userName);

        await messagesDataManager.SendMessage(userName, $"{nameof(RestoreBackup)} mediator.Send command",
            cancellationToken);

        var result = await mediator.Send(command, cancellationToken);

        await messagesDataManager.SendMessage(userName, $"{nameof(RestoreBackup)} finished", cancellationToken);
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }

    // GET api/database/testconnection/{databaseName?}
    private static async Task<IResult> TestConnection([FromRoute] string? databaseName,
        ICurrentUserByApiKey currentUserByApiKey, IMediator mediator, IMessagesDataManager messagesDataManager,
        CancellationToken cancellationToken = default)
    {
        var userName = currentUserByApiKey.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(TestConnection)} started", cancellationToken);
        Debug.WriteLine($"Call {nameof(TestConnectionCommandHandler)} from {nameof(TestConnection)}");

        //TestConnectionCommandRequest

        var command = TestConnectionCommandRequest.Create(databaseName, userName);
        var result = await mediator.Send(command, cancellationToken);

        await messagesDataManager.SendMessage(userName, $"{nameof(TestConnection)} finished", cancellationToken);
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }

    // POST api/database/updatestatistics/{databaseName}
    private static async Task<IResult> UpdateStatistics([FromRoute] string databaseName,
        ICurrentUserByApiKey currentUserByApiKey, IMediator mediator, IMessagesDataManager messagesDataManager,
        CancellationToken cancellationToken = default)
    {
        var userName = currentUserByApiKey.Name;
        await messagesDataManager.SendMessage(userName, $"{nameof(UpdateStatistics)} started", cancellationToken);
        Debug.WriteLine($"Call {nameof(UpdateStatisticsCommandHandler)} from {nameof(UpdateStatistics)}");

        //UpdateStatisticsCommandRequest

        var command = UpdateStatisticsCommandRequest.Create(databaseName, userName);
        var result = await mediator.Send(command, cancellationToken);

        await messagesDataManager.SendMessage(userName, $"{nameof(UpdateStatistics)} finished", cancellationToken);
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }
}