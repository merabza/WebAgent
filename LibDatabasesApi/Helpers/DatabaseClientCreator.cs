using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DatabasesManagement;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibWebAgentData;
using LibWebAgentData.ErrorModels;
using LibWebAgentData.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;

namespace LibDatabasesApi.Helpers;

public static class DatabaseClientCreator
{
    public static async Task<OneOf<IDatabaseApiClient, IEnumerable<Err>>> Create(IConfiguration config, ILogger logger,
        IMessagesDataManager? messagesDataManager, string? userName, CancellationToken cancellationToken)
    {
        var appSettings = AppSettings.Create(config);

        if (appSettings?.DatabaseServerData is null)
        {
            var err1 = DbApiErrors.DatabaseSettingsDoesNotSpecified;
            logger.LogError(err1.ErrorMessage);
            return new[] { err1 };
        }

        var dbServerData = appSettings.DatabaseServerData;

        var databaseManagementClient = await GetDatabaseConnectionSettings(logger, config, dbServerData,
            messagesDataManager, userName, cancellationToken);

        return databaseManagementClient is null
            ? new[] { DbApiErrors.ErrorCreateDatabaseConnection }
            : OneOf<IDatabaseApiClient, IEnumerable<Err>>.FromT0(databaseManagementClient);
    }


    private static async Task<IDatabaseApiClient?> GetDatabaseConnectionSettings(ILogger logger, IConfiguration config,
        DatabaseServerData databaseServerData, IMessagesDataManager? messagesDataManager, string? userName,
        CancellationToken cancellationToken)
    {
        var appSettings = AppSettings.Create(config);

        if (appSettings?.ApiClients is null)
            return null;

        var databaseManagementClient = await DatabaseAgentClientsFabric.CreateDatabaseManagementClient(false, logger,
            databaseServerData.DbWebAgentName, new ApiClients(appSettings.ApiClients),
            databaseServerData.DbConnectionName, new DatabaseServerConnections(appSettings.DatabaseServerConnections),
            messagesDataManager, userName, cancellationToken);
        return databaseManagementClient;
    }
}