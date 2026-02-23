using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibDatabaseParameters;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;
using ToolsManagement.DatabasesManagement;
using WebAgentShared.LibProjectsApi;
using WebAgentShared.LibWebAgentData;
using WebAgentShared.LibWebAgentData.ErrorModels;
using WebAgentShared.LibWebAgentData.Models;

namespace LibDatabasesApi.Helpers;

public static class DatabaseManagerCreator
{
    public static async ValueTask<OneOf<IDatabaseManager, Err[]>> Create(IConfiguration config, ILogger logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager? messagesDataManager, string? userName,
        CancellationToken cancellationToken = default)
    {
        var appSettings = AppSettings.Create(config);

        if (appSettings is null)
        {
            return await Task.FromResult(new[] { ProjectsErrors.AppSettingsIsNotCreated });
        }

        if (appSettings.DatabaseServerData is null)
        {
            Err err1 = DbApiErrors.DatabaseSettingsDoesNotSpecified;
            logger.LogError("{ErrorMessage}", err1.ErrorMessage);
            return new[] { err1 };
        }

        DatabaseServerData? dbServerData = appSettings.DatabaseServerData;

        return await GetDatabaseConnectionSettings(logger, httpClientFactory, config, dbServerData, messagesDataManager,
            userName, cancellationToken);
    }

    private static async ValueTask<OneOf<IDatabaseManager, Err[]>> GetDatabaseConnectionSettings(ILogger logger,
        IHttpClientFactory httpClientFactory, IConfiguration config, DatabaseServerData databaseServerData,
        IMessagesDataManager? messagesDataManager, string? userName, CancellationToken cancellationToken = default)
    {
        var appSettings = AppSettings.Create(config);

        if (appSettings is null)
        {
            return await Task.FromResult(new[] { ProjectsErrors.AppSettingsIsNotCreated });
        }

        OneOf<IDatabaseManager, Err[]> databaseManagementClient = await DatabaseManagersFactory.CreateDatabaseManager(
            logger, false, databaseServerData.DbConnectionName,
            new DatabaseServerConnections(appSettings.DatabaseServerConnections),
            new ApiClients(appSettings.ApiClients), httpClientFactory, messagesDataManager, userName,
            cancellationToken);
        return databaseManagementClient;
    }
}
