using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DatabaseTools.DbTools;
using LibDatabasesApi.CommandRequests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibFileParameters.Models;
using SystemTools.ApiContracts.Errors;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using ToolsManagement.DatabasesManagement;
using ToolsManagement.DatabasesManagement.Models;
using ToolsManagement.FileManagersMain;
using ToolsManagement.Installer.Errors;
using WebAgentContracts.WebAgentDatabasesApiContracts.Errors;
using WebAgentContracts.WebAgentDatabasesApiContracts.V1.Responses;
using WebAgentShared.LibProjectsApi;
using WebAgentShared.LibWebAgentData;
using WebAgentShared.LibWebAgentData.ErrorModels;
using WebAgentShared.LibWebAgentData.Models;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class RestoreBackupCommandHandler : ICommandHandler<RestoreBackupCommandRequestCommand>
{
    private readonly IApplication _application;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RestoreBackupCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public RestoreBackupCommandHandler(IConfiguration config, ILogger<RestoreBackupCommandHandler> logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager messagesDataManager, IApplication application)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
        _application = application;
    }

    public async Task<Result> Handle(RestoreBackupCommandRequestCommand request, CancellationToken cancellationToken)
    {
        var messageLogger = new MessageLogger(_logger, _messagesDataManager, request.UserName, false);

        await messageLogger.LogInfoAndSendMessage($"{nameof(RestoreBackupCommandHandler)} Handle started",
            cancellationToken);

        //შევამოწმოთ მოთხოვნის პარამეტრები: სახელი, პრეფიქსი, თარიღის ფორმატი, სუფიქსი
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Prefix) ||
            string.IsNullOrWhiteSpace(request.DateMask) || string.IsNullOrWhiteSpace(request.Suffix))
        {
            return ApiErrors.SomeRequestParametersAreNotValid;
        }

        await messageLogger.LogInfoAndSendMessage("Create AppSettings", cancellationToken);

        //ჩავტვირთოთ კონფიგურაცია
        var appSettings = AppSettings.Create(_config);
        if (appSettings is null)
        {
            return ProjectsErrors.AppSettingsIsNotCreated;
        }

        await messageLogger.LogInfoAndSendMessage("Checking database exchange settings", cancellationToken);

        //ბაზების გაცვლის პარამეტრების შემოწმება
        DatabasesBackupFilesExchangeParameters? databasesBackupFilesExchangeParameters =
            appSettings.DatabasesBackupFilesExchangeParameters;
        if (databasesBackupFilesExchangeParameters is null)
        {
            return DatabaseApiClientErrors.DatabasesBackupFilesExchangeParametersIsNotConfigured;
        }

        await messageLogger.LogInfoAndSendMessage("Checking database server settings", cancellationToken);

        //მონაცემთა ბაზის სერვერის პარამეტრების შემოწმება
        DatabaseServerData? databaseServerData = appSettings.DatabaseServerData;
        if (databaseServerData is null)
        {
            return DatabaseApiClientErrors.DatabaseServerDataIsNotConfigured;
        }

        var restoreDatabaseParameters = new DatabaseParameters
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

        var createBaseBackupParametersFactory = new CreateBaseBackupParametersFactory(_application.AppName, _logger,
            _messagesDataManager, request.UserName, false);

        await messageLogger.LogInfoAndSendMessage("Create Base Backup Parameters", cancellationToken);

        Result<BaseBackupParameters> createBaseBackupParametersResult =
            await createBaseBackupParametersFactory.CreateBaseBackupParameters(_httpClientFactory,
                restoreDatabaseParameters, databaseServerConnections, apiClients, fileStorages, smartSchemas,
                databasesBackupFilesExchangeParameters, cancellationToken);

        if (createBaseBackupParametersResult.IsFailure)
        {
            return new ValidationError([
                .. createBaseBackupParametersResult.Error.ToErrorArray(),
                DatabaseApiClientErrors.BaseBackupParametersIsNotCreated
            ]);
        }

        BaseBackupParameters createBaseBackupParameters = createBaseBackupParametersResult.Value;

        await messageLogger.LogInfoAndSendMessage("Create existing Database Backup", cancellationToken);

        var destinationBaseBackupRestorer = new BaseBackupRestoreTool(_logger, createBaseBackupParameters);
        await destinationBaseBackupRestorer.CreateDatabaseBackup(cancellationToken);

        FileManager? exchangeFileManager = createBaseBackupParameters.ExchangeFileManager;

        if (exchangeFileManager is null)
        {
            return await messageLogger.LogErrorAndSendMessageFromError(InstallerErrors.ExchangeFileManagerIsNull,
                cancellationToken);
        }

        string localArchiveFileName = Path.Combine(createBaseBackupParameters.LocalPath, request.Name);
        //თუ ფაილი უკვე მოქაჩულია, მეორედ მისი მოქაჩვა საჭირო არ არის
        if (!File.Exists(localArchiveFileName) && !exchangeFileManager.DownloadFile(request.Name,
                createBaseBackupParameters.DownloadTempExtension)) //მოვქაჩოთ არჩეული საინსტალაციო არქივი
        {
            return await messageLogger.LogErrorAndSendMessageFromError(
                InstallerErrors.ProjectArchiveFileWasNotDownloaded, cancellationToken);
        }

        var backupFileParameters = new BackupFileParameters(null, request.Name, request.Prefix, request.Suffix,
            request.DateMask);

        await messageLogger.LogInfoAndSendMessage("Restore Database from Backup", cancellationToken);

        if (!await destinationBaseBackupRestorer.RestoreDatabaseFromBackup(backupFileParameters,
                request.DatabaseRecoveryModel ?? EDatabaseRecoveryModel.Full, cancellationToken))
        {
            return DbApiErrors.CannotRestoreDatabase(request.DatabaseName, request.Name);
        }

        await messageLogger.LogInfoAndSendMessage("Finish Database Restore", cancellationToken);

        return Result.Success();
    }
}
