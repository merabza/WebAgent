﻿using System.Collections.Generic;
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
using MessagingAbstractions;
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

        var fromDatabaseParameters = new DatabasesParameters
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
        var localPath = databasesBackupFilesExchangeParameters.LocalPath;
        var downloadTempExtension = databasesBackupFilesExchangeParameters.DownloadTempExtension;
        var localSmartSchemaName = databasesBackupFilesExchangeParameters.LocalSmartSchemaName;
        var exchangeFileStorageName = databasesBackupFilesExchangeParameters.ExchangeFileStorageName;
        var uploadTempExtension = databasesBackupFilesExchangeParameters.UploadTempExtension;

        var createBaseBackupParametersFabric =
            new CreateBaseBackupParametersFabric(_logger, _messagesDataManager, request.UserName, false);
        var baseBackupRestoreParametersResult = await createBaseBackupParametersFabric.CreateBaseBackupParameters(
            _httpClientFactory, fromDatabaseParameters, databaseServerConnections, apiClients, fileStorages,
            smartSchemas, localPath, downloadTempExtension, localSmartSchemaName, exchangeFileStorageName,
            uploadTempExtension, cancellationToken);

        if (baseBackupRestoreParametersResult.IsT1)
            return Err.RecreateErrors(baseBackupRestoreParametersResult.AsT1,
                DatabaseApiClientErrors.BaseBackupParametersIsNotCreated);

        _logger.LogInformation("Create Backup for source Database");

        var sourceBaseBackupCreator = new BaseBackupRestorer(_logger, baseBackupRestoreParametersResult.AsT0);
        var backupFileParameters = await sourceBaseBackupCreator.CreateDatabaseBackup(cancellationToken);

        if (backupFileParameters is null)
            return await Task.FromResult(new[] { DatabaseApiClientErrors.BackupFileParametersIsNull });

        return backupFileParameters;


        //if (string.IsNullOrWhiteSpace(appSettings?.BaseBackupsLocalPatch))
        //    return new[] { DbApiErrors.BaseBackupsLocalPatchIsEmpty };

        //if (appSettings.DatabaseServerData is null)
        //{
        //    var err1 = DbApiErrors.DatabaseSettingsDoesNotSpecified;
        //    _logger.LogError(err1.ErrorMessage);
        //    return new[] { err1 };
        //}

        //var databaseServerData = appSettings.DatabaseServerData;


        ////თუ გაცვლის სერვერის პარამეტრები გვაქვს,
        ////შევქმნათ შესაბამისი ფაილმენეჯერი

        ////var exchangeFileStorageName = appSettings.BackupsExchangeStorage;
        //var (exchangeFileStorage, exchangeFileManager) = await FileManagersFabricExt.CreateFileStorageAndFileManager(
        //    false, _logger, appSettings.BaseBackupsLocalPatch, exchangeFileStorageName, fileStorages,
        //    _messagesDataManager, request.UserName, cancellationToken);

        ////წყაროს ფაილსაცავი
        //var databaseBackupsFileStorageName = databaseServerData.DatabaseBackupsFileStorageName;
        //var (databaseBackupsFileStorage, databaseBackupsFileManager) =
        //    await FileManagersFabricExt.CreateFileStorageAndFileManager(false, _logger,
        //        appSettings.BaseBackupsLocalPatch, databaseBackupsFileStorageName, fileStorages, _messagesDataManager,
        //        request.UserName, cancellationToken);

        //if (databaseBackupsFileManager == null)
        //    return new[] { DbApiErrors.DatabaseBackupsFileManagerDoesNotCreated };

        //if (databaseBackupsFileStorage == null)
        //    return new[] { DbApiErrors.DatabaseBackupsFileStorageDoesNotCreated };

        //var createDatabaseManagerResult = await DatabaseManagersFabric.CreateDatabaseManager(_logger,
        //    _httpClientFactory, false, databaseServerData.DbConnectionName,
        //    new DatabaseServerConnections(appSettings.DatabaseServerConnections),
        //    new ApiClients(appSettings.ApiClients), _messagesDataManager, request.UserName, cancellationToken);

        //if (createDatabaseManagerResult.IsT1)
        //    return Err.RecreateErrors(createDatabaseManagerResult.AsT1,
        //        DbApiErrors.DatabaseManagementClientDoesNotCreated);

        ////var result = DatabaseBackupParametersDomainCreator.Create(request);

        ////DatabaseBackupParametersDomain databaseBackupParametersDomain;
        ////if (result.IsT0)
        ////    databaseBackupParametersDomain = result.AsT0;
        ////else
        ////    return result.AsT1.ToArray();

        ////if (databaseBackupParametersDomain is null)
        ////    return new[] { DbApiErrors.CreateBackupRequestIsInvalid };

        ////databaseBackupParametersDomain
        //var backupFileParametersResult = await createDatabaseManagerResult.AsT0.CreateBackup(request.DatabaseName,
        //    request.DbServerFoldersSetName, cancellationToken);

        //if (backupFileParametersResult.IsT1)
        //    return Err.RecreateErrors(backupFileParametersResult.AsT1, DbApiErrors.BackupDoesNotCreated);

        //var backupFileParameters = backupFileParametersResult.AsT0;

        //var needDownloadFromSource = !FileStorageData.IsSameToLocal(databaseBackupsFileStorage,
        //    appSettings.BaseBackupsLocalPatch); //, _messagesDataManager, request.UserName


        //if (needDownloadFromSource)
        //{
        //    //წყაროდან ლოკალურ ფოლდერში მოქაჩვა
        //    if (!databaseBackupsFileManager.DownloadFile(backupFileParameters.Name, ".down!"))
        //        return new[] { DbApiErrors.CanNotDownloadFile(backupFileParameters.Name) };


        //    var downloadSmartSchema = smartSchemas.GetSmartSchemaByKey(databaseServerData.DbSmartSchemaName);
        //    //if (downloadSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება
        //    databaseBackupsFileManager.RemoveRedundantFiles(backupFileParameters.Prefix, backupFileParameters.DateMask,
        //        backupFileParameters.Suffix, downloadSmartSchema);

        //    var localSmartSchema = smartSchemas.GetSmartSchemaByKey(appSettings.LocalSmartSchemaName);
        //    //if (localSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება
        //    var localFileManager =
        //        FileManagersFabric.CreateFileManager(false, _logger, appSettings.BaseBackupsLocalPatch);

        //    localFileManager?.RemoveRedundantFiles(backupFileParameters.Prefix, backupFileParameters.DateMask,
        //        backupFileParameters.Suffix, localSmartSchema);
        //}

        ////ან თუ გაცვლის ფაილსაცავი არ გვაქვს, ან ლოკალურია და მისი ფოლდერი ემთხვევა პარამეტრების ლოკალურ ფოლდერს.
        ////   მაშინ მოქაჩვა საჭირო აღარ არის
        ////, _messagesDataManager, request.UserName
        //var needUploadToExchange = exchangeFileManager is not null && exchangeFileStorage is not null &&
        //                           !FileStorageData.IsSameToLocal(exchangeFileStorage,
        //                               appSettings.BaseBackupsLocalPatch) &&
        //                           exchangeFileStorageName != databaseBackupsFileStorageName;


        //if (!needUploadToExchange)
        //    return backupFileParameters;

        //if (exchangeFileManager is null)
        //    return new[] { DbApiErrors.ExchangeFileManagerDoesNotCreated };

        //if (!exchangeFileManager.UploadFile(backupFileParameters.Name, ".up!"))
        //    return new[] { DbApiErrors.CanNotUploadFile(backupFileParameters.Name) };

        //var exchangeSmartSchema = smartSchemas.GetSmartSchemaByKey(appSettings.BackupsExchangeStorageSmartSchemaName);
        ////if (exchangeSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება
        //exchangeFileManager.RemoveRedundantFiles(backupFileParameters.Prefix, backupFileParameters.DateMask,
        //    backupFileParameters.Suffix, exchangeSmartSchema);

        //return backupFileParameters;
    }
}