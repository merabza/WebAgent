using CliParametersApiClientsEdit.Models;
using CliParametersDataEdit.Models;
using CliParametersEdit.Fabrics;
using CliToolsData.Models;
using DatabaseApiClients;
using DatabaseManagementClients;
using DatabaseManagementClients.Models;
using FileManagersMain;
using LibWebAgentData;
using LibWebAgentData.Contracts.V1;
using LibWebAgentData.ErrorModels;
using LibWebAgentData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;

namespace LibDatabases.Controllers.V1;

public class DatabaseController : Controller
{
    private readonly ApiKeysChecker _apiKeysChecker;
    private readonly AppSettings _appSettings;
    private readonly IConfiguration _config;

    private readonly ILogger<DatabaseController> _logger;

    public DatabaseController(ILogger<DatabaseController> logger, IConfiguration config)
    {
        _logger = logger;
        _appSettings = AppSettings.Create(config);
        _config = config;
        _apiKeysChecker = new ApiKeysChecker(_logger, config);
    }

    private OneOf<DatabaseServerData, Err>
        Check(string serverName, string apiKey)
    {
        var remoteAddress = Request.HttpContext.Connection.RemoteIpAddress;

        if (remoteAddress is null)
            return Errors.CannotDetectRemoteAddress;

        if (!_apiKeysChecker.Check(apiKey, remoteAddress.MapToIPv4().ToString()))
            return Errors.ApiKeyIsInvalid;

        if (!_appSettings.DatabaseServers.ContainsKey(serverName))
        {
            _logger.LogError("Source database settings with name {serverName} does not specified", serverName);
            return Errors.InvalidServerName;
        }

        return _appSettings.DatabaseServers[serverName];
    }

    private string? Check(string apiKey)
    {
        var remoteAddress = Request.HttpContext.Connection.RemoteIpAddress;

        if (remoteAddress is null)
            return "Cannot detect Remote address";

        return !_apiKeysChecker.Check(apiKey, remoteAddress.MapToIPv4().ToString())
            ? "API Key is invalid"
            : null;
    }

    private DatabaseManagementClient? GetDatabaseConnectionSettings(DatabaseServerData databaseServerData)
    {
        var databaseManagementClient =
            DatabaseAgentClientsFabric.CreateDatabaseManagementClient(false, _logger,
                databaseServerData.DbWebAgentName, new ApiClients(_appSettings.ApiClients),
                databaseServerData.DbConnectionName,
                new DatabaseServerConnections(_appSettings.DatabaseServerConnections));
        return databaseManagementClient;
    }

    // POST api/database/checkrepairdatabase
    [HttpPost(ApiRoutes.Database.CheckRepairDatabase)]
    public async Task<ActionResult> CheckRepairDatabase(string serverName, string databaseName,
        [FromQuery] string apiKey)
    {
        try
        {
            var checkResult = Check(serverName, apiKey);

            if (checkResult.IsT1)
                return BadRequest(checkResult.AsT1);

            var databaseManagementClient = GetDatabaseConnectionSettings(checkResult.AsT0);
            if (databaseManagementClient is null)
                return BadRequest(Errors.ErrorCreateDatabaseConnection);
            if (await databaseManagementClient.CheckRepairDatabase(serverName, databaseName))
                return Ok();


            return BadRequest(Errors.CannotCheckAndRepairDatabase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, null);
            return BadRequest(
                $"Cannot check and repair Database {databaseName} on server {serverName}, because of error {ex.Message}");
        }
    }

    // POST api/database/createbackup
    [HttpPost(ApiRoutes.Database.CreateBackup)]
    public async Task<ActionResult<BackupFileParameters>>
        CreateBackup(string serverName, string databaseName) //, [FromBody] ApiKeyModel apiKey
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_appSettings.BaseBackupsLocalPatch))
                return BadRequest(Errors.BaseBackupsLocalPatchIsEmpty);

            var apiKeyWithDatabaseBackupParametersModel =
                await _apiKeysChecker.DeserializeAsync<ApiKeyWithDatabaseBackupParametersModel>(Request.Body);

            if (apiKeyWithDatabaseBackupParametersModel?.DatabaseBackupParameters == null)
                return BadRequest(Errors.InvalidDatabaseBackupParameters);

            var checkResult = Check(serverName, apiKeyWithDatabaseBackupParametersModel.ApiKey);

            if (checkResult.IsT1)
                return BadRequest(checkResult.AsT1);

            var databaseServerData = checkResult.AsT0;

            var fileStorages = FileStorages.Create(_config);

            //თუ გაცვლის სერვერის პარამეტრები გვაქვს,
            //შევქმნათ შესაბამისი ფაილმენეჯერი
            var exchangeFileStorageName = _appSettings.BackupsExchangeStorage;
            (var exchangeFileStorage, var exchangeFileManager) =
                FileManagersFabricExt.CreateFileStorageAndFileManager(false, _logger,
                    _appSettings.BaseBackupsLocalPatch, exchangeFileStorageName, fileStorages);

            //წყაროს ფაილსაცავი
            var databaseBackupsFileStorageName = databaseServerData.DatabaseBackupsFileStorageName;
            (var databaseBackupsFileStorage, var databaseBackupsFileManager) =
                FileManagersFabricExt.CreateFileStorageAndFileManager(false, _logger,
                    _appSettings.BaseBackupsLocalPatch, databaseBackupsFileStorageName, fileStorages);

            if (databaseBackupsFileManager == null)
                return BadRequest(Errors.FileManagerDoesNotCreated);

            if (databaseBackupsFileStorage == null)
                return BadRequest(Errors.FileStorageDoesNotCreated);


            var databaseManagementClient =
                DatabaseAgentClientsFabric.CreateDatabaseManagementClient(false, _logger,
                    databaseServerData.DbWebAgentName, new ApiClients(_appSettings.ApiClients),
                    databaseServerData.DbConnectionName,
                    new DatabaseServerConnections(_appSettings.DatabaseServerConnections));

            if (databaseManagementClient is null)
                return BadRequest(Errors.DatabaseManagementClientDoesNotCreated);


            var backupFileParameters = databaseManagementClient
                .CreateBackup(apiKeyWithDatabaseBackupParametersModel.DatabaseBackupParameters, databaseName).Result;

            if (backupFileParameters == null)
                return BadRequest("Backup not Created");

            var needDownloadFromSource =
                !FileStorageData.IsSameToLocal(databaseBackupsFileStorage, _appSettings.BaseBackupsLocalPatch);

            SmartSchemas smartSchemas = new(_appSettings.SmartSchemas);


            if (needDownloadFromSource)
            {
                //წყაროდან ლოკალურ ფოლდერში მოქაჩვა
                if (!databaseBackupsFileManager.DownloadFile(backupFileParameters.Name, ".down!"))
                    return BadRequest($"Can not Download File {backupFileParameters.Name}");

                var downloadSmartSchema =
                    smartSchemas.GetSmartSchemaByKey(databaseServerData.DbSmartSchemaName);
                //if (downloadSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება
                databaseBackupsFileManager.RemoveRedundantFiles(backupFileParameters.Prefix,
                    backupFileParameters.DateMask,
                    backupFileParameters.Suffix, downloadSmartSchema);

                var localSmartSchema = smartSchemas.GetSmartSchemaByKey(_appSettings.LocalSmartSchemaName);
                //if (localSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება
                var localFileManager =
                    FileManagersFabric.CreateFileManager(false, _logger, _appSettings.BaseBackupsLocalPatch);

                if (localFileManager is not null)
                    localFileManager.RemoveRedundantFiles(backupFileParameters.Prefix, backupFileParameters.DateMask,
                        backupFileParameters.Suffix, localSmartSchema);
            }

            //ან თუ გაცვლის ფაილსაცავი არ გვაქვს, ან ლოკალურია და მისი ფოლდერი ემთხვევა პარამეტრების ლოკალურ ფოლდერს.
            //   მაშინ მოქაჩვა საჭირო აღარ არის
            var needUploadToExchange = exchangeFileManager is not null && exchangeFileStorage is not null &&
                                       !FileStorageData.IsSameToLocal(exchangeFileStorage,
                                           _appSettings.BaseBackupsLocalPatch) && exchangeFileStorageName !=
                                       databaseBackupsFileStorageName;

            if (!needUploadToExchange)
                return Ok(backupFileParameters);

            if (exchangeFileManager is null)
                return BadRequest("exchangeFileManager does not created");

            if (!exchangeFileManager.UploadFile(backupFileParameters.Name, ".up!"))
                return BadRequest($"Can not Upload File {backupFileParameters.Name}");

            var exchangeSmartSchema =
                smartSchemas.GetSmartSchemaByKey(_appSettings.BackupsExchangeStorageSmartSchemaName);
            //if (exchangeSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება
            exchangeFileManager.RemoveRedundantFiles(backupFileParameters.Prefix, backupFileParameters.DateMask,
                backupFileParameters.Suffix, exchangeSmartSchema);

            return Ok(backupFileParameters);
        }
        catch (Exception e)
        {
            _logger.LogError(e, null);
            return BadRequest("error");
        }
    }

    // POST api/database/executecommand
    [HttpPost(ApiRoutes.Database.ExecuteCommand)]
    public async Task<ActionResult> ExecuteCommand(string serverName, string databaseName,
        [FromQuery] string apiKey)
    {
        try
        {
            var checkResult = Check(serverName, apiKey);

            if (checkResult.IsT1)
                return BadRequest(checkResult.AsT1);

            var commandText = await _apiKeysChecker.DeserializeAsync<string>(Request.Body);
            if (string.IsNullOrWhiteSpace(commandText))
                return BadRequest("CommandText is not valid");

            var databaseManagementClient = GetDatabaseConnectionSettings(checkResult.AsT0);
            if (databaseManagementClient is null)
                return BadRequest("databaseManagementClient does not created");
            if (await databaseManagementClient.ExecuteCommand(commandText, serverName, databaseName))
                return Ok();

            return BadRequest($"Cannot check and repair Database {databaseName} on server {serverName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, null);
            return BadRequest(
                $"Cannot check and repair Database {databaseName} on server {serverName}, because of error {ex.Message}");
        }
    }

    // GET api/database/getdatabasenames
    [HttpGet(ApiRoutes.Database.GetDatabaseNames)]
    public async Task<ActionResult<bool>> GetDatabaseNames(string serverName, [FromQuery] string apiKey)
    {
        try
        {
            var checkResult = Check(serverName, apiKey);

            if (checkResult.IsT1)
                return BadRequest(checkResult.AsT1);

            var databaseServerData = checkResult.AsT0;

            var agentClient = DatabaseAgentClientsFabric.CreateDatabaseManagementClient(false,
                _logger,
                databaseServerData.DbWebAgentName, new ApiClients(_appSettings.ApiClients),
                databaseServerData.DbConnectionName,
                new DatabaseServerConnections(_appSettings.DatabaseServerConnections));

            if (agentClient is null)
                return BadRequest("agentClient does not created");

            return Ok(await agentClient.GetDatabaseNames(serverName));
        }
        catch (Exception e)
        {
            _logger.LogError(e, null);
            return BadRequest("error");
        }
    }

    // GET api/database/getdatabaseservernames
    [HttpGet(ApiRoutes.Database.GetDatabaseServerNames)]
    public ActionResult<List<string>> GetDatabaseServerNames([FromQuery] string apiKey)
    {
        try
        {
            var message = Check(apiKey);

            if (message != null)
                return BadRequest(message);

            return Ok(_appSettings.DatabaseServers.Keys.ToList());
        }
        catch (Exception e)
        {
            _logger.LogError(e, null);
            return BadRequest("error");
        }
    }

    // GET api/database/isdatabaseexists
    [HttpGet(ApiRoutes.Database.IsDatabaseExists)]
    public async Task<ActionResult<bool>> IsDatabaseExists(string serverName, string databaseName,
        [FromQuery] string apiKey)
    {
        try
        {
            var checkResult = Check(serverName, apiKey);

            if (checkResult.IsT1)
                return BadRequest(checkResult.AsT1);

            var databaseServerData = checkResult.AsT0;

            var agentClient = DatabaseAgentClientsFabric.CreateDatabaseManagementClient(false,
                _logger,
                databaseServerData.DbWebAgentName, new ApiClients(_appSettings.ApiClients),
                databaseServerData.DbConnectionName,
                new DatabaseServerConnections(_appSettings.DatabaseServerConnections));

            if (agentClient is null)
                return BadRequest("agentClient does not created");

            return await agentClient.IsDatabaseExists(serverName, databaseName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, null);
            return BadRequest("error");
        }
    }

    [HttpPut(ApiRoutes.Database.RestoreBackup)]
    public async Task<ActionResult> RestoreBackup(string serverName, string databaseName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_appSettings.BaseBackupsLocalPatch))
                return BadRequest(Errors.BaseBackupsLocalPatchIsEmpty);

            var apiKeyWithBackupFileParameters =
                await _apiKeysChecker.DeserializeAsync<ApiKeyWithBackupFileParameters>(Request.Body);

            if (apiKeyWithBackupFileParameters is null)
                return BadRequest("invalid apiKeyWithBackupFileParameters");

            var backupFileParameters = apiKeyWithBackupFileParameters.BackupFileParameters;
            if (string.IsNullOrWhiteSpace(backupFileParameters.Name))
                return BadRequest("Invalid Backup File Parameters");

            var checkResult = Check(serverName, apiKeyWithBackupFileParameters.ApiKey);

            if (checkResult.IsT1)
                return BadRequest(checkResult.AsT1);

            var databaseServerData = checkResult.AsT0;

            var fileStorages = FileStorages.Create(_config);

            var agentClient = DatabaseAgentClientsFabric.CreateDatabaseManagementClient(false,
                _logger,
                databaseServerData.DbWebAgentName, new ApiClients(_appSettings.ApiClients),
                databaseServerData.DbConnectionName,
                new DatabaseServerConnections(_appSettings.DatabaseServerConnections));

            if (agentClient is null)
                return BadRequest("agentClient does not created");

            //თუ გაცვლის სერვერის პარამეტრები გვაქვს,
            //შევქმნათ შესაბამისი ფაილმენეჯერი
            var exchangeFileStorageName = _appSettings.BackupsExchangeStorage;
            (var exchangeFileStorage, var exchangeFileManager) =
                FileManagersFabricExt.CreateFileStorageAndFileManager(false, _logger,
                    _appSettings.BaseBackupsLocalPatch, exchangeFileStorageName, fileStorages);

            //წყაროს ფაილსაცავი
            var databaseBackupsFileStorageName = databaseServerData.DatabaseBackupsFileStorageName;
            (var databaseBackupsFileStorage, var databaseBackupsFileManager) =
                FileManagersFabricExt.CreateFileStorageAndFileManager(false, _logger,
                    _appSettings.BaseBackupsLocalPatch, databaseBackupsFileStorageName, fileStorages);

            if (databaseBackupsFileStorage is null)
                return BadRequest("databaseBackupsFileStorage does not created");

            if (databaseBackupsFileManager is null)
                return BadRequest("Cannot find Source File databaseBackupsFileManager");

            var localFileManager =
                FileManagersFabric.CreateFileManager(false, _logger, _appSettings.BaseBackupsLocalPatch);

            var needDownloadFromExchange = exchangeFileStorage != null &&
                                           !FileStorageData.IsSameToLocal(exchangeFileStorage,
                                               _appSettings.BaseBackupsLocalPatch);

            if (needDownloadFromExchange)
            {
                if (localFileManager is null)
                    return BadRequest("localFileManager does not created");

                if (exchangeFileManager is null)
                    return BadRequest("exchangeFileManager does not created");

                //წყაროდან ლოკალურ ფოლდერში მოქაჩვა
                if (!exchangeFileManager.DownloadFile(backupFileParameters.Name, ".down!"))
                {
                    var message = "can not receive backup from exchange storage";
                    _logger.LogError(message);
                    return BadRequest(message);
                }

                SmartSchemas smartSchemas = new(_appSettings.SmartSchemas);

                var exchangeSmartSchema =
                    smartSchemas.GetSmartSchemaByKey(_appSettings.BackupsExchangeStorageSmartSchemaName);
                //if (exchangeSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება
                exchangeFileManager.RemoveRedundantFiles(backupFileParameters.Prefix, backupFileParameters.DateMask,
                    backupFileParameters.Suffix, exchangeSmartSchema);

                var localSmartSchema = smartSchemas.GetSmartSchemaByKey(_appSettings.LocalSmartSchemaName);
                //if (localSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება

                localFileManager.RemoveRedundantFiles(backupFileParameters.Prefix, backupFileParameters.DateMask,
                    backupFileParameters.Suffix, localSmartSchema);
            }

            var needUploadDatabaseBackupsStorage =
                !FileStorageData.IsSameToLocal(databaseBackupsFileStorage, _appSettings.BaseBackupsLocalPatch);

            if (needUploadDatabaseBackupsStorage)
            {
                if (!databaseBackupsFileManager.UploadFile(backupFileParameters.Name, ".up!"))
                {
                    var message = $"Can not Upload File {backupFileParameters.Name}";
                    _logger.LogError(message);
                    return BadRequest(message);
                }

                SmartSchemas smartSchemas = new(_appSettings.SmartSchemas);

                var dbFilesSmartSchema =
                    smartSchemas.GetSmartSchemaByKey(databaseServerData.DbSmartSchemaName);
                //if (downloadSmartSchema != null)// ეს შემოწმება საჭირო იქნება, თუ დასაშვები იქნება ჭკვიანი სქემის არ მითითება
                databaseBackupsFileManager.RemoveRedundantFiles(backupFileParameters.Prefix,
                    backupFileParameters.DateMask,
                    backupFileParameters.Suffix, dbFilesSmartSchema);
            }

            var success =
                await agentClient.RestoreDatabaseFromBackup(backupFileParameters, serverName, databaseName);
            if (success)
                return Ok();

            return BadRequest(
                $"Cannot restore database {databaseName} from file {backupFileParameters.Name} on server {serverName}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, null);
            return BadRequest("error");
        }
    }


    // POST api/database/recompileprocedures
    [HttpPost(ApiRoutes.Database.RecompileProcedures)]
    public async Task<ActionResult> RecompileProcedures(string serverName, string databaseName,
        [FromQuery] string apiKey)
    {
        try
        {
            var checkResult = Check(serverName, apiKey);

            if (checkResult.IsT1)
                return BadRequest(checkResult.AsT1);

            var databaseServerData = checkResult.AsT0;

            var databaseAgentClient = GetDatabaseConnectionSettings(databaseServerData);

            if (databaseAgentClient is null)
                return BadRequest("databaseAgentClient does not created");

            if (await databaseAgentClient.RecompileProcedures(serverName, databaseName))
                return Ok();

            return BadRequest($"Cannot Recompile Procedures for Database {databaseName} on server {serverName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, null);
            return BadRequest(
                $"Cannot Recompile Procedures for Database {databaseName} on server {serverName}, because of error {ex.Message}");
        }
    }


    // GET api/database/testconnection
    [HttpGet(ApiRoutes.Database.TestConnection)]
    public async Task<ActionResult> TestConnection(string serverName, string databaseName,
        [FromQuery] string apiKey)
    {
        try
        {
            var checkResult = Check(serverName, apiKey);

            if (checkResult.IsT1)
                return BadRequest(checkResult.AsT1);

            var databaseServerData = checkResult.AsT0;

            var databaseAgentClient = GetDatabaseConnectionSettings(databaseServerData);

            if (databaseAgentClient is null)
                return BadRequest("databaseAgentClient does not created");

            if (await databaseAgentClient.TestConnection(serverName, databaseName))
                return Ok();

            return BadRequest($"Cannot Update Statistics for Database {databaseName} on server {serverName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, null);
            return BadRequest(
                $"Cannot Update Statistics for Database {databaseName} on server {serverName}, because of error {ex.Message}");
        }
    }


    // POST api/database/updatestatistics
    [HttpPost(ApiRoutes.Database.UpdateStatistics)]
    public async Task<ActionResult> UpdateStatistics(string serverName, string databaseName,
        [FromQuery] string apiKey)
    {
        try
        {
            var checkResult = Check(serverName, apiKey);

            var databaseServerData = checkResult.AsT0;

            var databaseAgentClient = GetDatabaseConnectionSettings(databaseServerData);

            if (databaseAgentClient is null)
                return BadRequest("databaseAgentClient does not created");

            if (await databaseAgentClient.UpdateStatistics(serverName, databaseName))
                return Ok();

            return BadRequest($"Cannot Update Statistics for Database {databaseName} on server {serverName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, null);
            return BadRequest(
                $"Cannot Update Statistics for Database {databaseName} on server {serverName}, because of error {ex.Message}");
        }
    }
}