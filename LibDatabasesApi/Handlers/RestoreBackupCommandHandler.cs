using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiContracts.Errors;
using DatabasesManagement;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibDatabasesApi.CommandRequests;
using LibFileParameters.Models;
using LibProjectsApi;
using LibWebAgentData;
using LibWebAgentData.ErrorModels;
using MediatR;
using MessagingAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;
using SystemToolsShared.Errors;
using WebAgentDatabasesApiContracts.Errors;
using WebAgentDatabasesApiContracts.V1.Responses;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class RestoreBackupCommandHandler : ICommandHandler<RestoreBackupCommandRequest>
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RestoreBackupCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public RestoreBackupCommandHandler(IConfiguration config, ILogger<RestoreBackupCommandHandler> logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(RestoreBackupCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        await _messagesDataManager.SendMessage(request.UserName,
            $"{nameof(RestoreBackupCommandHandler)} Handle started", cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Prefix) ||
            string.IsNullOrWhiteSpace(request.DateMask) || string.IsNullOrWhiteSpace(request.Suffix))
            return new[] { ApiErrors.SomeRequestParametersAreNotValid };

        await _messagesDataManager.SendMessage(request.UserName, "Create AppSettings", cancellationToken);

        var appSettings = AppSettings.Create(_config);
        if (appSettings is null)
            return new[] { ProjectsErrors.AppSettingsIsNotCreated };

        var databasesBackupFilesExchangeParameters = appSettings.DatabasesBackupFilesExchangeParameters;
        if (databasesBackupFilesExchangeParameters is null)
            return new[] { DatabaseApiClientErrors.DatabasesBackupFilesExchangeParametersIsNotConfigured };

        var databaseServerData = appSettings.DatabaseServerData;
        if (databaseServerData is null)
            return await Task.FromResult(new[] { DatabaseApiClientErrors.DatabaseServerDataIsNotConfigured });

        var restoreDatabaseParameters = new DatabasesParameters
        {
            DatabaseName = request.DatabaseName,
            DbServerFoldersSetName = request.DbServerFoldersSetName,
            DbConnectionName = databaseServerData.DbConnectionName,
            FileStorageName = databaseServerData.DatabaseBackupsFileStorageName,
            SmartSchemaName = databaseServerData.DbSmartSchemaName
        };

        var databaseServerConnections = new DatabaseServerConnections(appSettings.DatabaseServerConnections);
        var apiClients = new ApiClients(appSettings.ApiClients);
        var smartSchemas = new SmartSchemas(appSettings.SmartSchemas);
        var fileStorages = new FileStorages(appSettings.FileStorages);
        var localPath = databasesBackupFilesExchangeParameters.LocalPath;
        var downloadTempExtension = databasesBackupFilesExchangeParameters.DownloadTempExtension;
        var localSmartSchemaName = databasesBackupFilesExchangeParameters.LocalSmartSchemaName;
        var exchangeFileStorageName = databasesBackupFilesExchangeParameters.ExchangeFileStorageName;
        var uploadTempExtension = databasesBackupFilesExchangeParameters.UploadTempExtension;

        var createBaseBackupParametersFabric =
            new CreateBaseBackupParametersFabric(_logger, _messagesDataManager, request.UserName, false);

        var createBaseBackupParametersResult = await createBaseBackupParametersFabric.CreateBaseBackupParameters(
            _httpClientFactory, restoreDatabaseParameters, databaseServerConnections, apiClients, fileStorages,
            smartSchemas, localPath, downloadTempExtension, localSmartSchemaName, exchangeFileStorageName,
            uploadTempExtension, cancellationToken);

        if (createBaseBackupParametersResult.IsT1)
            return Err.RecreateErrors(createBaseBackupParametersResult.AsT1,
                DatabaseApiClientErrors.BaseBackupParametersIsNotCreated);

        var destinationBaseBackupRestorer = new BaseBackupRestorer(_logger, createBaseBackupParametersResult.AsT0);
        await destinationBaseBackupRestorer.CreateDatabaseBackup(cancellationToken);

        var backupFileParameters = new BackupFileParameters(null, request.Name, request.Prefix, request.Suffix,
            request.DateMask);

        if (!await destinationBaseBackupRestorer.RestoreDatabaseFromBackup(backupFileParameters, cancellationToken))
            return new[] { DbApiErrors.CannotRestoreDatabase(request.DatabaseName, request.Name) };

        return new Unit();
    }
}