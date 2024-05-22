using System.Collections.Generic;
using System.Net.Http;
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
using SignalRContracts;
using SystemToolsShared;

namespace LibDatabasesApi.Helpers;

public static class DatabaseClientCreator
{
    public static async Task<OneOf<IDatabaseManager, IEnumerable<Err>>> Create(IConfiguration config, ILogger logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager? messagesDataManager, string? userName,
        CancellationToken cancellationToken)
    {
        var appSettings = AppSettings.Create(config);

        if (appSettings?.DatabaseServerData is null)
        {
            var err1 = DbApiErrors.DatabaseSettingsDoesNotSpecified;
            logger.LogError(err1.ErrorMessage);
            return new[] { err1 };
        }

        var dbServerData = appSettings.DatabaseServerData;

        var databaseManagementClient = await GetDatabaseConnectionSettings(logger, httpClientFactory, config,
            dbServerData, messagesDataManager, userName, cancellationToken);

        return databaseManagementClient is null
            ? new[] { DbApiErrors.ErrorCreateDatabaseConnection }
            : OneOf<IDatabaseManager, IEnumerable<Err>>.FromT0(databaseManagementClient);
    }


    private static async Task<IDatabaseManager?> GetDatabaseConnectionSettings(ILogger logger,
        IHttpClientFactory httpClientFactory, IConfiguration config, DatabaseServerData databaseServerData,
        IMessagesDataManager? messagesDataManager, string? userName, CancellationToken cancellationToken)
    {
        var appSettings = AppSettings.Create(config);

        if (appSettings?.ApiClients is null)
            return null;

        var databaseManagementClient = await DatabaseAgentClientsFabric.CreateDatabaseManager(false, logger,
            httpClientFactory, databaseServerData.DbWebAgentName, new ApiClients(appSettings.ApiClients),
            databaseServerData.DbConnectionName, new DatabaseServerConnections(appSettings.DatabaseServerConnections),
            messagesDataManager, userName, cancellationToken);
        return databaseManagementClient;
    }
}