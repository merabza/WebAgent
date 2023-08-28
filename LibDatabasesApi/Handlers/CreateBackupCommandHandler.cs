using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DatabaseApiClients;
using FileManagersMain;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibDatabasesApi.CommandRequests;
using LibFileParameters.Models;
using LibWebAgentData;
using LibWebAgentData.ErrorModels;
using MessagingAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;
using WebAgentProjectsApiContracts.V1.Responses;

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CreateBackupCommandHandler : ICommandHandler<CreateBackupCommandRequest, BackupFileParameters>
{
    private readonly IConfiguration _config;
    private readonly ILogger<CreateBackupCommandHandler> _logger;
    private readonly IMessagesDataManager? _messagesDataManager;

    public CreateBackupCommandHandler(IConfiguration config, ILogger<CreateBackupCommandHandler> logger,
        IMessagesDataManager? messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<BackupFileParameters, IEnumerable<Err>>> Handle(CreateBackupCommandRequest request,
        CancellationToken cancellationToken)
    {
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

        if (databaseBackupsFileManager == null)
            return new[] { DbApiErrors.DatabaseBackupsFileManagerDoesNotCreated };

        if (databaseBackupsFileStorage == null)
            return new[] { DbApiErrors.DatabaseBackupsFileStorageDoesNotCreated };

        var databaseManagementClient = DatabaseAgentClientsFabric.CreateDatabaseManagementClient(false, _logger,
            databaseServerData.DbWebAgentName, new ApiClients(appSettings.ApiClients),
            databaseServerData.DbConnectionName, new DatabaseServerConnections(appSettings.DatabaseServerConnections),
            _messagesDataManager, request.UserName);

        if (databaseManagementClient is null)
            return new[] { DbApiErrors.DatabaseManagementClientDoesNotCreated };

        var result = DatabaseBackupParametersDomainCreator.Create(request);

        DatabaseBackupParametersDomain databaseBackupParametersDomain;
        if (result.IsT0)
            databaseBackupParametersDomain = result.AsT0;
        else
            return result.AsT1.ToArray();

        if (databaseBackupParametersDomain is null)
            return new[] { DbApiErrors.CreateBackupRequestIsInvalid };

        var backupFileParameters =
            await databaseManagementClient.CreateBackup(databaseBackupParametersDomain, request.DatabaseName);

        if (backupFileParameters == null)
            return new[] { DbApiErrors.BackupDoesNotCreated };

        var needDownloadFromSource = !FileStorageData.IsSameToLocal(databaseBackupsFileStorage,
            appSettings.BaseBackupsLocalPatch, _messagesDataManager, request.UserName);

        SmartSchemas smartSchemas = new(appSettings.SmartSchemas);


        if (needDownloadFromSource)
        {
            //წყაროდან ლოკალურ ფოლდერში მოქაჩვა
            if (!databaseBackupsFileManager.DownloadFile(backupFileParameters.Name, ".down!"))
                return new[] { DbApiErrors.CanNotDownloadFile(backupFileParameters.Name) };


            var downloadSmartSchema =
                smartSchemas.GetSmartSchemaByKey(databaseServerData.DbSmartSchemaName);
            //if (downloadSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება
            databaseBackupsFileManager.RemoveRedundantFiles(backupFileParameters.Prefix,
                backupFileParameters.DateMask,
                backupFileParameters.Suffix, downloadSmartSchema);

            var localSmartSchema = smartSchemas.GetSmartSchemaByKey(appSettings.LocalSmartSchemaName);
            //if (localSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება
            var localFileManager =
                FileManagersFabric.CreateFileManager(false, _logger, appSettings.BaseBackupsLocalPatch);

            localFileManager?.RemoveRedundantFiles(backupFileParameters.Prefix, backupFileParameters.DateMask,
                backupFileParameters.Suffix, localSmartSchema);
        }

        //ან თუ გაცვლის ფაილსაცავი არ გვაქვს, ან ლოკალურია და მისი ფოლდერი ემთხვევა პარამეტრების ლოკალურ ფოლდერს.
        //   მაშინ მოქაჩვა საჭირო აღარ არის
        var needUploadToExchange = exchangeFileManager is not null && exchangeFileStorage is not null &&
                                   !FileStorageData.IsSameToLocal(exchangeFileStorage,
                                       appSettings.BaseBackupsLocalPatch, _messagesDataManager, request.UserName) &&
                                   exchangeFileStorageName != databaseBackupsFileStorageName;

        if (!needUploadToExchange)
            return backupFileParameters;

        if (exchangeFileManager is null)
            return new[] { DbApiErrors.ExchangeFileManagerDoesNotCreated };

        if (!exchangeFileManager.UploadFile(backupFileParameters.Name, ".up!"))
            return new[] { DbApiErrors.CanNotUploadFile(backupFileParameters.Name) };

        var exchangeSmartSchema =
            smartSchemas.GetSmartSchemaByKey(appSettings.BackupsExchangeStorageSmartSchemaName);
        //if (exchangeSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება
        exchangeFileManager.RemoveRedundantFiles(backupFileParameters.Prefix, backupFileParameters.DateMask,
            backupFileParameters.Suffix, exchangeSmartSchema);

        return backupFileParameters;
    }
}