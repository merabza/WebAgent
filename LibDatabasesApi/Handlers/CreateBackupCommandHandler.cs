using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DatabasesManagement;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibDatabasesApi.CommandRequests;
using LibFileParameters.Models;
using LibProjectsApi;
using LibWebAgentData;
using MediatRMessagingAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;
using SystemToolsShared.Errors;
using WebAgentDatabasesApiContracts.Errors;
using WebAgentDatabasesApiContracts.V1.Responses;

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CreateBackupCommandHandler : ICommandHandler<CreateBackupCommandRequest, BackupFileParameters>
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CreateBackupCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CreateBackupCommandHandler(IConfiguration config, ILogger<CreateBackupCommandHandler> logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<BackupFileParameters, IEnumerable<Err>>> Handle(CreateBackupCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        var appSettings = AppSettings.Create(_config);
        if (appSettings is null)
            return await Task.FromResult(new[] { ProjectsErrors.AppSettingsIsNotCreated });

        var databasesBackupFilesExchangeParameters = appSettings.DatabasesBackupFilesExchangeParameters;
        if (databasesBackupFilesExchangeParameters is null)
            return await Task.FromResult(new[]
            {
                DatabaseApiClientErrors.DatabasesBackupFilesExchangeParametersIsNotConfigured
            });

        var databaseServerData = appSettings.DatabaseServerData;
        if (databaseServerData is null)
            return await Task.FromResult(new[] { DatabaseApiClientErrors.DatabaseServerDataIsNotConfigured });

        var fromDatabaseParameters = new DatabaseParameters
        {
            DatabaseName = request.DatabaseName,
            DbServerFoldersSetName = request.DbServerFoldersSetName,
            DbConnectionName = databaseServerData.DbConnectionName,
            FileStorageName = databaseServerData.DatabaseBackupsFileStorageName,
            SmartSchemaName = databaseServerData.DbSmartSchemaName
        };

        var databaseServerConnections = new DatabaseServerConnections(appSettings.DatabaseServerConnections);
        var apiClients = new ApiClients(appSettings.ApiClients);
        var fileStorages = new FileStorages(appSettings.FileStorages);
        var smartSchemas = new SmartSchemas(appSettings.SmartSchemas);
        //var localPath = databasesBackupFilesExchangeParameters.LocalPath;
        //var downloadTempExtension = databasesBackupFilesExchangeParameters.DownloadTempExtension;
        //var localSmartSchemaName = databasesBackupFilesExchangeParameters.LocalSmartSchemaName;
        //var exchangeFileStorageName = databasesBackupFilesExchangeParameters.ExchangeFileStorageName;
        //var uploadTempExtension = databasesBackupFilesExchangeParameters.UploadTempExtension;

        var createBaseBackupParametersFabric =
            new CreateBaseBackupParametersFabric(_logger, _messagesDataManager, request.UserName, false);
        var baseBackupRestoreParametersResult = await createBaseBackupParametersFabric.CreateBaseBackupParameters(
            _httpClientFactory, fromDatabaseParameters, databaseServerConnections, apiClients, fileStorages,
            smartSchemas, databasesBackupFilesExchangeParameters, cancellationToken);

        if (baseBackupRestoreParametersResult.IsT1)
            return Err.RecreateErrors(baseBackupRestoreParametersResult.AsT1,
                DatabaseApiClientErrors.BaseBackupParametersIsNotCreated);

        _logger.LogInformation("Create Backup for source Database");

        var sourceBaseBackupCreator = new BaseBackupRestorer(_logger, baseBackupRestoreParametersResult.AsT0);
        var backupFileParameters = await sourceBaseBackupCreator.CreateDatabaseBackup(cancellationToken);

        if (backupFileParameters is null)
            return await Task.FromResult(new[] { DatabaseApiClientErrors.BackupFileParametersIsNull });

        return backupFileParameters;
    }
}