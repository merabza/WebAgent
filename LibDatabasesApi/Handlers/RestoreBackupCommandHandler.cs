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

        var restoreDatabaseParameters = new DatabasesParameters
        {
            DatabaseName = request.DatabaseName, DbServerFoldersSetName = request.DbServerFoldersSetName
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


        //if (string.IsNullOrWhiteSpace(databasesBackupFilesExchangeParameters.LocalPath))
        //    return new[] { DbApiErrors.BaseBackupsLocalPatchIsEmpty };

        //if (appSettings.DatabaseServerData is null)
        //{
        //    var err1 = DbApiErrors.DatabaseSettingsDoesNotSpecified;
        //    _logger.LogError(err1.ErrorMessage);
        //    return new[] { err1 };
        //}

        //var databaseServerData = appSettings.DatabaseServerData;

        //var fileStorages = new FileStorages(appSettings.FileStorages);

        //await _messagesDataManager.SendMessage(request.UserName, "Create CreateDatabaseManagementClient",
        //    cancellationToken);

        //var createDatabaseManagerResult = await DatabaseManagersFabric.CreateDatabaseManager(_logger,
        //    _httpClientFactory, false, databaseServerData.DbConnectionName,
        //    new DatabaseServerConnections(appSettings.DatabaseServerConnections),
        //    new ApiClients(appSettings.ApiClients), _messagesDataManager, request.UserName, cancellationToken);

        //if (createDatabaseManagerResult.IsT1)
        //    return Err.RecreateErrors(createDatabaseManagerResult.AsT1,
        //        DbApiErrors.DatabaseManagementClientDoesNotCreated);

        ////თუ გაცვლის სერვერის პარამეტრები გვაქვს,
        ////შევქმნათ შესაბამისი ფაილმენეჯერი
        //var exchangeFileStorageName = appSettings.BackupsExchangeStorage;
        //await _messagesDataManager.SendMessage(request.UserName,
        //    $"Create CreateFileStorageAndFileManager {appSettings.BaseBackupsLocalPatch} - {exchangeFileStorageName}",
        //    cancellationToken);

        //var (exchangeFileStorage, exchangeFileManager) = await FileManagersFabricExt.CreateFileStorageAndFileManager(
        //    false, _logger, appSettings.BaseBackupsLocalPatch, exchangeFileStorageName, fileStorages,
        //    _messagesDataManager, request.UserName, cancellationToken);

        ////წყაროს ფაილსაცავი
        //var databaseBackupsFileStorageName = databaseServerData.DatabaseBackupsFileStorageName;
        //await _messagesDataManager.SendMessage(request.UserName,
        //    $"Create CreateFileStorageAndFileManager {appSettings.BaseBackupsLocalPatch} - {databaseBackupsFileStorageName}",
        //    cancellationToken);

        //var (databaseBackupsFileStorage, databaseBackupsFileManager) =
        //    await FileManagersFabricExt.CreateFileStorageAndFileManager(false, _logger,
        //        appSettings.BaseBackupsLocalPatch, databaseBackupsFileStorageName, fileStorages, _messagesDataManager,
        //        request.UserName, cancellationToken);

        //if (databaseBackupsFileStorage is null)
        //    return new[] { DbApiErrors.DatabaseBackupsFileManagerDoesNotCreated };

        //if (databaseBackupsFileManager is null)
        //    return new[] { DbApiErrors.DatabaseBackupsFileStorageDoesNotCreated };

        //await _messagesDataManager.SendMessage(request.UserName,
        //    $"Create CreateFileManager {appSettings.BaseBackupsLocalPatch}", cancellationToken);

        //var localFileManager = FileManagersFabric.CreateFileManager(false, _logger, appSettings.BaseBackupsLocalPatch);

        ////, _messagesDataManager, request.UserName
        //var needDownloadFromExchange = exchangeFileStorage != null &&
        //                               !FileStorageData.IsSameToLocal(exchangeFileStorage,
        //                                   appSettings.BaseBackupsLocalPatch);

        //if (needDownloadFromExchange)
        //{
        //    if (localFileManager is null)
        //        return new[] { DbApiErrors.LocalFileManagerDoesNotCreated };

        //    if (exchangeFileManager is null)
        //        return new[] { DbApiErrors.ExchangeFileManagerDoesNotCreated };

        //    await _messagesDataManager.SendMessage(request.UserName, $"DownloadFile {request.Name}", cancellationToken);

        //    //წყაროდან ლოკალურ ფოლდერში მოქაჩვა
        //    if (!exchangeFileManager.DownloadFile(request.Name, ".down!"))
        //    {
        //        var err = DbApiErrors.LocalFileManagerDoesNotCreated;
        //        _logger.LogError(err.ErrorMessage);
        //        return new[] { err };
        //    }

        //    SmartSchemas smartSchemas = new(appSettings.SmartSchemas);

        //    var exchangeSmartSchema =
        //        smartSchemas.GetSmartSchemaByKey(appSettings.BackupsExchangeStorageSmartSchemaName);
        //    //if (exchangeSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება
        //    exchangeFileManager.RemoveRedundantFiles(request.Prefix, request.DateMask, request.Suffix,
        //        exchangeSmartSchema);

        //    var localSmartSchema = smartSchemas.GetSmartSchemaByKey(appSettings.LocalSmartSchemaName);
        //    //if (localSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება

        //    localFileManager.RemoveRedundantFiles(request.Prefix, request.DateMask, request.Suffix, localSmartSchema);
        //}

        ////, _messagesDataManager, request.UserName
        //var needUploadDatabaseBackupsStorage =
        //    !FileStorageData.IsSameToLocal(databaseBackupsFileStorage, appSettings.BaseBackupsLocalPatch);

        //if (needUploadDatabaseBackupsStorage)
        //{
        //    await _messagesDataManager.SendMessage(request.UserName, $"UploadFile {request.Name}", cancellationToken);

        //    if (!databaseBackupsFileManager.UploadFile(request.Name, ".up!"))
        //    {
        //        var err = DbApiErrors.CanNotUploadFile(request.Name);
        //        _logger.LogError(err.ErrorMessage);
        //        return new[] { err };
        //    }

        //    SmartSchemas smartSchemas = new(appSettings.SmartSchemas);

        //    var dbFilesSmartSchema = smartSchemas.GetSmartSchemaByKey(databaseServerData.DbSmartSchemaName);
        //    //if (downloadSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება
        //    databaseBackupsFileManager.RemoveRedundantFiles(request.Prefix, request.DateMask, request.Suffix,
        //        dbFilesSmartSchema);
        //}

        //await _messagesDataManager.SendMessage(request.UserName, $"RestoreDatabaseFromBackup {request.DatabaseName}",
        //    cancellationToken);

        //var restoreDatabaseFromBackupResult = await createDatabaseManagerResult.AsT0.RestoreDatabaseFromBackup(
        //    new BackupFileParameters(null, request.Name, request.Prefix, request.Suffix, request.DateMask),
        //    request.DatabaseName, request.DbServerFoldersSetName, null, cancellationToken);
        //if (restoreDatabaseFromBackupResult.IsSome)
        //    return Err.RecreateErrors((Err[])restoreDatabaseFromBackupResult,
        //        DbApiErrors.CannotRestoreDatabase(request.DatabaseName, request.Name));
        return new Unit();
    }
}