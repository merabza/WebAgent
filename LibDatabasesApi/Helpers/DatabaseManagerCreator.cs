using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibDatabaseParameters;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using ToolsManagement.DatabasesManagement;
using WebAgentShared.LibProjectsApi;
using WebAgentShared.LibWebAgentData;
using WebAgentShared.LibWebAgentData.ErrorModels;
using WebAgentShared.LibWebAgentData.Models;

namespace LibDatabasesApi.Helpers;

public static class DatabaseManagerCreator
{
    public static async ValueTask<Result<IDatabaseManager>> Create(string appName, IConfiguration config,
        ILogger logger, IHttpClientFactory httpClientFactory, IMessagesDataManager? messagesDataManager,
        string? userName, CancellationToken cancellationToken = default)
    {
        var appSettings = AppSettings.Create(config);

        if (appSettings is null)
        {
            return ProjectsErrors.AppSettingsIsNotCreated;
        }

        if (appSettings.DatabaseServerData is null)
        {
            Error err1 = DbApiErrors.DatabaseSettingsDoesNotSpecified;
            logger.LogError("{Description}", err1.Description);
            return err1;
        }

        DatabaseServerData? dbServerData = appSettings.DatabaseServerData;

        return await GetDatabaseConnectionSettings(appName, logger, httpClientFactory, config, dbServerData,
            messagesDataManager, userName, cancellationToken);
    }

    private static async ValueTask<Result<IDatabaseManager>> GetDatabaseConnectionSettings(string appName,
        ILogger logger, IHttpClientFactory httpClientFactory, IConfiguration config,
        DatabaseServerData databaseServerData, IMessagesDataManager? messagesDataManager, string? userName,
        CancellationToken cancellationToken = default)
    {
        var appSettings = AppSettings.Create(config);

        if (appSettings is null)
        {
            return ProjectsErrors.AppSettingsIsNotCreated;
        }

        return await DatabaseManagersFactory.CreateDatabaseManager(appName, logger, false,
            databaseServerData.DbConnectionName, new DatabaseServerConnections(appSettings.DatabaseServerConnections),
            new ApiClients(appSettings.ApiClients), httpClientFactory, messagesDataManager, userName,
            cancellationToken);
    }
}
