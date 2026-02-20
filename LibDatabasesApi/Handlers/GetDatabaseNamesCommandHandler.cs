using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DatabaseTools.DbTools.Models;
using LibDatabasesApi.CommandRequests;
using LibDatabasesApi.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class
    GetDatabaseNamesCommandHandler : ICommandHandler<GetDatabaseNamesRequestCommand, DatabaseInfoModel[]>
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GetDatabaseNamesCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public GetDatabaseNamesCommandHandler(IConfiguration config, ILogger<GetDatabaseNamesCommandHandler> logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<DatabaseInfoModel[], Err[]>> Handle(GetDatabaseNamesRequestCommand request,
        CancellationToken cancellationToken)
    {
        var result = await DatabaseManagerCreator.Create(_config, _logger, _httpClientFactory, _messagesDataManager,
            request.UserName, cancellationToken);
        if (result.IsT1)
        {
            return result.AsT1.ToArray();
        }

        var databaseManagementClient = result.AsT0;

        var getDatabaseNamesResult = await databaseManagementClient.GetDatabaseNames(cancellationToken);
        return getDatabaseNamesResult.Match<OneOf<DatabaseInfoModel[], Err[]>>(f0 => f0.ToArray(), f1 => f1);

        //ასეთი კონსტრუქცია ვერ გავმართე
        //return await Task.FromResult(result.Match(x => x.GetDatabaseNames(request.ServerName).Result, er => er.ToArray()));
    }
}
