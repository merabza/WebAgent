using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiContracts.Errors;
using DatabasesManagement;
using DbTools;
using Installer.Errors;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibDatabasesApi.CommandRequests;
using LibFileParameters.Models;
using LibProjectsApi;
using LibWebAgentData;
using LibWebAgentData.ErrorModels;
using MediatR;
using MediatRMessagingAbstractions;
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
public sealed class RestoreBackupCommandHandler : ICommandHandler<RestoreBackupCommandRequestCommand>
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

    public async Task<OneOf<Unit, Err[]>> Handle(RestoreBackupCommandRequestCommand request,
        CancellationToken cancellationToken = default)
    {
        var messageLogger = new MessageLogger(_logger, _messagesDataManager, request.UserName, false);

        await messageLogger.LogInfoAndSendMessage($"{nameof(RestoreBackupCommandHandler)} Handle started",
            cancellationToken);

        //შევამოწმოთ მოთხოვნის პარამეტრები: სახელი, პრეფიქსი, თარიღის ფორმატი, სუფიქსი
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Prefix) ||
            string.IsNullOrWhiteSpace(request.DateMask) || string.IsNullOrWhiteSpace(request.Suffix))
            return new[] { ApiErrors.SomeRequestParametersAreNotValid };

        await messageLogger.LogInfoAndSendMessage("Create AppSettings", cancellationToken);

        //ჩავტვირთოთ კონფიგურაცია
        var appSettings = AppSettings.Create(_config);
        if (appSettings is null)
            return new[] { ProjectsErrors.AppSettingsIsNotCreated };

        //ბაზების გაცვლის პარამეტრების შემოწმება
        var databasesBackupFilesExchangeParameters = appSettings.DatabasesBackupFilesExchangeParameters;
        if (databasesBackupFilesExchangeParameters is null)
            return new[] { DatabaseApiClientErrors.DatabasesBackupFilesExchangeParametersIsNotConfigured };

        //მონაცემთა ბაზის სერვერის პარამეტრების შემოწმება
        var databaseServerData = appSettings.DatabaseServerData;
        if (databaseServerData is null)
            return await Task.FromResult(new[] { DatabaseApiClientErrors.DatabaseServerDataIsNotConfigured });

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

        var createBaseBackupParametersFactory =
            new CreateBaseBackupParametersFactory(_logger, _messagesDataManager, request.UserName, false);

        var createBaseBackupParametersResult = await createBaseBackupParametersFactory.CreateBaseBackupParameters(
            _httpClientFactory, restoreDatabaseParameters, databaseServerConnections, apiClients, fileStorages,
            smartSchemas, databasesBackupFilesExchangeParameters, cancellationToken);

        if (createBaseBackupParametersResult.IsT1)
            return Err.RecreateErrors(createBaseBackupParametersResult.AsT1,
                DatabaseApiClientErrors.BaseBackupParametersIsNotCreated);

        var createBaseBackupParameters = createBaseBackupParametersResult.AsT0;

        var destinationBaseBackupRestorer = new BaseBackupRestoreTool(_logger, createBaseBackupParameters);
        await destinationBaseBackupRestorer.CreateDatabaseBackup(cancellationToken);

        var exchangeFileManager = createBaseBackupParameters.ExchangeFileManager;

        if (exchangeFileManager is null)
            return await messageLogger.LogErrorAndSendMessageFromError(InstallerErrors.ExchangeFileManagerIsNull,
                cancellationToken);

        var localArchiveFileName = Path.Combine(createBaseBackupParameters.LocalPath, request.Name);
        //თუ ფაილი უკვე მოქაჩულია, მეორედ მისი მოქაჩვა საჭირო არ არის
        if (!File.Exists(localArchiveFileName) && !exchangeFileManager.DownloadFile(request.Name,
                createBaseBackupParameters.DownloadTempExtension)) //მოვქაჩოთ არჩეული საინსტალაციო არქივი
            return await messageLogger.LogErrorAndSendMessageFromError(
                InstallerErrors.ProjectArchiveFileWasNotDownloaded, cancellationToken);

        var backupFileParameters = new BackupFileParameters(null, request.Name, request.Prefix, request.Suffix,
            request.DateMask);

        if (!await destinationBaseBackupRestorer.RestoreDatabaseFromBackup(backupFileParameters,
                request.DatabaseRecoveryModel ?? EDatabaseRecoveryModel.Full, cancellationToken))
            return new[] { DbApiErrors.CannotRestoreDatabase(request.DatabaseName, request.Name) };

        return new Unit();
    }
}