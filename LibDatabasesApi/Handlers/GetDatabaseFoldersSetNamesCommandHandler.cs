using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LibDatabasesApi.CommandRequests;
using LibDatabasesApi.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using ToolsManagement.DatabasesManagement;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class
    GetDatabaseFoldersSetNamesCommandHandler : ICommandHandler<GetDatabaseFoldersSetNamesRequestCommand, string[]>
{
    private readonly IApplication _application;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GetDatabaseFoldersSetNamesCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public GetDatabaseFoldersSetNamesCommandHandler(IConfiguration config,
        ILogger<GetDatabaseFoldersSetNamesCommandHandler> logger, IHttpClientFactory httpClientFactory,
        IMessagesDataManager messagesDataManager, IApplication application)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
        _application = application;
    }

    public async Task<Result<string[]>> Handle(GetDatabaseFoldersSetNamesRequestCommand request,
        CancellationToken cancellationToken)
    {
        Result<IDatabaseManager> result = await DatabaseManagerCreator.Create(_application.AppName, _config, _logger,
            _httpClientFactory, _messagesDataManager, request.UserName, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error;
        }

        IDatabaseManager databaseManagementClient = result.Value;

        Result<List<string>> getDatabaseFoldersSetNamesResult =
            await databaseManagementClient.GetDatabaseFoldersSetNames(cancellationToken);
        if (getDatabaseFoldersSetNamesResult.IsFailure)
        {
            return getDatabaseFoldersSetNamesResult.Error;
        }

        return getDatabaseFoldersSetNamesResult.Value.ToArray();
    }
}
