using DatabaseApiClients;
using DatabaseManagementClients;
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
    public static OneOf<DatabaseManagementClient, IEnumerable<Err>> Create(IConfiguration config, ILogger logger)
    {
        var appSettings = AppSettings.Create(config);

        if (appSettings?.DatabaseServerData is null)
        {
            var err1 = DbApiErrors.DatabaseSettingsDoesNotSpecified;
            logger.LogError(err1.ErrorMessage);
            return new[] { err1 };
        }

        var dbServerData = appSettings.DatabaseServerData;

        var databaseManagementClient = GetDatabaseConnectionSettings(logger, config, dbServerData);
        if (databaseManagementClient is null)
            return new[] { DbApiErrors.ErrorCreateDatabaseConnection };


        return databaseManagementClient;
    }


    private static DatabaseManagementClient? GetDatabaseConnectionSettings(ILogger logger,
        IConfiguration config, DatabaseServerData databaseServerData)
    {
        var appSettings = AppSettings.Create(config);

        if (appSettings?.ApiClients is null)
            return null;

        var databaseManagementClient =
            DatabaseAgentClientsFabric.CreateDatabaseManagementClient(false, logger,
                databaseServerData.DbWebAgentName, new ApiClients(appSettings.ApiClients),
                databaseServerData.DbConnectionName,
                new DatabaseServerConnections(appSettings.DatabaseServerConnections));
        return databaseManagementClient;
    }
}