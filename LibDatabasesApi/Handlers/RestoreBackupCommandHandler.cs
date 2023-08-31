using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApiToolsShared;
using DatabasesManagement;
using FileManagersMain;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibDatabasesApi.CommandRequests;
using LibFileParameters.Models;
using LibWebAgentData;
using LibWebAgentData.ErrorModels;
using MediatR;
using MessagingAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;
using WebAgentProjectsApiContracts.V1.Responses;

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class RestoreBackupCommandHandler : ICommandHandler<RestoreBackupCommandRequest>
{
    private readonly IConfiguration _config;
    private readonly ILogger<RestoreBackupCommandHandler> _logger;
    private readonly IMessagesDataManager? _messagesDataManager;

    public RestoreBackupCommandHandler(IConfiguration config, ILogger<RestoreBackupCommandHandler> logger,
        IMessagesDataManager? messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(RestoreBackupCommandRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Prefix) ||
            string.IsNullOrWhiteSpace(request.DateMask) || string.IsNullOrWhiteSpace(request.Suffix))
            return new[] { ApiErrors.SomeRequestParametersAreNotValid };

        var appSettings = AppSettings.Create(_config);

        if (string.IsNullOrWhiteSpace(appSettings?.BaseBackupsLocalPatch))
            return new[] { DbApiErrors.BaseBackupsLocalPatchIsEmpty };

        if (appSettings.DatabaseServerData is null)
        {
            var err1 = DbApiErrors.DatabaseSettingsDoesNotSpecified;
            _logger.LogError(err1.ErrorMessage);
            return new[] { err1 };
        }

        var databaseServerData = appSettings.DatabaseServerData;

        var fileStorages = FileStorages.Create(_config);

        var databaseManagementClient = DatabaseAgentClientsFabric.CreateDatabaseManagementClient(false, _logger,
            databaseServerData.DbWebAgentName, new ApiClients(appSettings.ApiClients),
            databaseServerData.DbConnectionName, new DatabaseServerConnections(appSettings.DatabaseServerConnections),
            _messagesDataManager, request.UserName);

        if (databaseManagementClient is null)
            return new[] { DbApiErrors.DatabaseManagementClientDoesNotCreated };

        //თუ გაცვლის სერვერის პარამეტრები გვაქვს,
        //შევქმნათ შესაბამისი ფაილმენეჯერი
        var exchangeFileStorageName = appSettings.BackupsExchangeStorage;
        var (exchangeFileStorage, exchangeFileManager) =
            FileManagersFabricExt.CreateFileStorageAndFileManager(false, _logger,
                appSettings.BaseBackupsLocalPatch, exchangeFileStorageName, fileStorages, _messagesDataManager,
                request.UserName);

        //წყაროს ფაილსაცავი
        var databaseBackupsFileStorageName = databaseServerData.DatabaseBackupsFileStorageName;
        var (databaseBackupsFileStorage, databaseBackupsFileManager) =
            FileManagersFabricExt.CreateFileStorageAndFileManager(false, _logger,
                appSettings.BaseBackupsLocalPatch, databaseBackupsFileStorageName, fileStorages, _messagesDataManager,
                request.UserName);

        if (databaseBackupsFileStorage is null)
            return new[] { DbApiErrors.DatabaseBackupsFileManagerDoesNotCreated };

        if (databaseBackupsFileManager is null)
            return new[] { DbApiErrors.DatabaseBackupsFileStorageDoesNotCreated };

        var localFileManager =
            FileManagersFabric.CreateFileManager(false, _logger, appSettings.BaseBackupsLocalPatch);

        var needDownloadFromExchange = exchangeFileStorage != null &&
                                       !FileStorageData.IsSameToLocal(exchangeFileStorage,
                                           appSettings.BaseBackupsLocalPatch, _messagesDataManager, request.UserName);

        if (needDownloadFromExchange)
        {
            if (localFileManager is null)
                return new[] { DbApiErrors.LocalFileManagerDoesNotCreated };

            if (exchangeFileManager is null)
                return new[] { DbApiErrors.ExchangeFileManagerDoesNotCreated };

            //წყაროდან ლოკალურ ფოლდერში მოქაჩვა
            if (!exchangeFileManager.DownloadFile(request.Name, ".down!"))
            {
                var err = DbApiErrors.LocalFileManagerDoesNotCreated;
                _logger.LogError(err.ErrorMessage);
                return new[] { err };
            }

            SmartSchemas smartSchemas = new(appSettings.SmartSchemas);

            var exchangeSmartSchema =
                smartSchemas.GetSmartSchemaByKey(appSettings.BackupsExchangeStorageSmartSchemaName);
            //if (exchangeSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება
            exchangeFileManager.RemoveRedundantFiles(request.Prefix, request.DateMask,
                request.Suffix, exchangeSmartSchema);

            var localSmartSchema = smartSchemas.GetSmartSchemaByKey(appSettings.LocalSmartSchemaName);
            //if (localSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება

            localFileManager.RemoveRedundantFiles(request.Prefix, request.DateMask,
                request.Suffix, localSmartSchema);
        }

        var needUploadDatabaseBackupsStorage = !FileStorageData.IsSameToLocal(databaseBackupsFileStorage,
            appSettings.BaseBackupsLocalPatch, _messagesDataManager, request.UserName);

        if (needUploadDatabaseBackupsStorage)
        {
            if (!databaseBackupsFileManager.UploadFile(request.Name, ".up!"))
            {
                var err = DbApiErrors.CanNotUploadFile(request.Name);
                _logger.LogError(err.ErrorMessage);
                return new[] { err };
            }

            SmartSchemas smartSchemas = new(appSettings.SmartSchemas);

            var dbFilesSmartSchema =
                smartSchemas.GetSmartSchemaByKey(databaseServerData.DbSmartSchemaName);
            //if (downloadSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება
            databaseBackupsFileManager.RemoveRedundantFiles(request.Prefix,
                request.DateMask,
                request.Suffix, dbFilesSmartSchema);
        }

        var success = await databaseManagementClient.RestoreDatabaseFromBackup(
            new BackupFileParameters(request.Name, request.Prefix,
                request.Suffix, request.DateMask), request.DatabaseName);
        return success ? new Unit() : new[] { DbApiErrors.CannotRestoreDatabase(request.DatabaseName, request.Name) };
    }
}