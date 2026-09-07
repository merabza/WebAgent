using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DatabaseTools.DbTools.Models;
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
    GetDatabaseNamesCommandHandler : ICommandHandler<GetDatabaseNamesRequestCommand, DatabaseInfoModel[]>
{
    private readonly IApplication _application;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GetDatabaseNamesCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public GetDatabaseNamesCommandHandler(IConfiguration config, ILogger<GetDatabaseNamesCommandHandler> logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager messagesDataManager, IApplication application)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
        _application = application;
    }

    public async Task<Result<DatabaseInfoModel[]>> Handle(GetDatabaseNamesRequestCommand request,
        CancellationToken cancellationToken)
    {
        Result<IDatabaseManager> result = await DatabaseManagerCreator.Create(_application.AppName, _config, _logger,
            _httpClientFactory, _messagesDataManager, request.UserName, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error;
        }

        IDatabaseManager databaseManagementClient = result.Value;

        Result<List<DatabaseInfoModel>> getDatabaseNamesResult =
            await databaseManagementClient.GetDatabaseNames(cancellationToken);
        if (getDatabaseNamesResult.IsFailure)
        {
            return getDatabaseNamesResult.Error;
        }

        return getDatabaseNamesResult.Value.ToArray();
    }
}
