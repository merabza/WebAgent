using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LibDatabasesApi.CommandRequests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibFileParameters.Models;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;
using ToolsManagement.DatabasesManagement;
using ToolsManagement.DatabasesManagement.Models;
using WebAgentContracts.WebAgentDatabasesApiContracts.Errors;
using WebAgentContracts.WebAgentDatabasesApiContracts.V1.Responses;
using WebAgentShared.LibProjectsApi;
using WebAgentShared.LibWebAgentData;
using WebAgentShared.LibWebAgentData.Models;

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CreateBackupCommandHandler : ICommandHandler<CreateBackupRequestCommand, BackupFileParameters>
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

    public async Task<OneOf<BackupFileParameters, Err[]>> Handle(CreateBackupRequestCommand request,
        CancellationToken cancellationToken)
    {
        var appSettings = AppSettings.Create(_config);
        if (appSettings is null)
        {
            return await Task.FromResult(new[] { ProjectsErrors.AppSettingsIsNotCreated });
        }

        DatabasesBackupFilesExchangeParameters? databasesBackupFilesExchangeParameters =
            appSettings.DatabasesBackupFilesExchangeParameters;
        if (databasesBackupFilesExchangeParameters is null)
        {
            return await Task.FromResult(new[]
            {
                DatabaseApiClientErrors.DatabasesBackupFilesExchangeParametersIsNotConfigured
            });
        }

        DatabaseServerData? databaseServerData = appSettings.DatabaseServerData;
        if (databaseServerData is null)
        {
            return await Task.FromResult(new[] { DatabaseApiClientErrors.DatabaseServerDataIsNotConfigured });
        }

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

        var createBaseBackupParametersFactory =
            new CreateBaseBackupParametersFactory(_logger, _messagesDataManager, request.UserName, false);
        OneOf<BaseBackupParameters, Err[]> baseBackupRestoreParametersResult =
            await createBaseBackupParametersFactory.CreateBaseBackupParameters(_httpClientFactory,
                fromDatabaseParameters, databaseServerConnections, apiClients, fileStorages, smartSchemas,
                databasesBackupFilesExchangeParameters, cancellationToken);

        if (baseBackupRestoreParametersResult.IsT1)
        {
            return Err.RecreateErrors(baseBackupRestoreParametersResult.AsT1,
                DatabaseApiClientErrors.BaseBackupParametersIsNotCreated);
        }

        _logger.LogInformation("Create Backup for source Database");

        var sourceBaseBackupCreator = new BaseBackupRestoreTool(_logger, baseBackupRestoreParametersResult.AsT0);
        BackupFileParameters? backupFileParameters =
            await sourceBaseBackupCreator.CreateDatabaseBackup(cancellationToken);

        if (backupFileParameters is null)
        {
            return await Task.FromResult(new[] { DatabaseApiClientErrors.BackupFileParametersIsNull });
        }

        return backupFileParameters;
    }
}
