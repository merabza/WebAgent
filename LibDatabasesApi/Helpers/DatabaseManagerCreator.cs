using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DatabasesManagement;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibProjectsApi;
using LibWebAgentData;
using LibWebAgentData.ErrorModels;
using LibWebAgentData.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;
using SystemToolsShared.Errors;

namespace LibDatabasesApi.Helpers;

public static class DatabaseManagerCreator
{
    public static async ValueTask<OneOf<IDatabaseManager, Err[]>> Create(IConfiguration config, ILogger logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager? messagesDataManager, string? userName,
        CancellationToken cancellationToken = default)
    {
        var appSettings = AppSettings.Create(config);

        if (appSettings is null)
            return await Task.FromResult(new[] { ProjectsErrors.AppSettingsIsNotCreated });

        if (appSettings.DatabaseServerData is null)
        {
            var err1 = DbApiErrors.DatabaseSettingsDoesNotSpecified;
            logger.LogError(err1.ErrorMessage);
            return new[] { err1 };
        }

        var dbServerData = appSettings.DatabaseServerData;

        return await GetDatabaseConnectionSettings(logger, httpClientFactory, config, dbServerData, messagesDataManager,
            userName, cancellationToken);
    }

    private static async ValueTask<OneOf<IDatabaseManager, Err[]>> GetDatabaseConnectionSettings(ILogger logger,
        IHttpClientFactory httpClientFactory, IConfiguration config, DatabaseServerData databaseServerData,
        IMessagesDataManager? messagesDataManager, string? userName, CancellationToken cancellationToken = default)
    {
        var appSettings = AppSettings.Create(config);

        if (appSettings is null)
            return await Task.FromResult(new[] { ProjectsErrors.AppSettingsIsNotCreated });

        var databaseManagementClient = await DatabaseManagersFactory.CreateDatabaseManager(logger, false,
            databaseServerData.DbConnectionName, new DatabaseServerConnections(appSettings.DatabaseServerConnections),
            new ApiClients(appSettings.ApiClients), httpClientFactory, messagesDataManager, userName,
            cancellationToken);
        return databaseManagementClient;
    }
}