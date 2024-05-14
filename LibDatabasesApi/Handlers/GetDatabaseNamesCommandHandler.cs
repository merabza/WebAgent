using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DbTools.Models;
using LibDatabasesApi.CommandRequests;
using LibDatabasesApi.Helpers;
using MessagingAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SignalRContracts;
using SystemToolsShared;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class
    GetDatabaseNamesCommandHandler : ICommandHandler<GetDatabaseNamesCommandRequest, IEnumerable<DatabaseInfoModel>>
{
    private readonly IConfiguration _config;
    private readonly ILogger<GetDatabaseNamesCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public GetDatabaseNamesCommandHandler(IConfiguration config, ILogger<GetDatabaseNamesCommandHandler> logger,
        IMessagesDataManager messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<IEnumerable<DatabaseInfoModel>, IEnumerable<Err>>> Handle(
        GetDatabaseNamesCommandRequest request, CancellationToken cancellationToken)
    {
        var result = await DatabaseClientCreator.Create(_config, _logger, _messagesDataManager, request.UserName,
            cancellationToken);
        if (result.IsT1)
            return result.AsT1.ToArray();
        var databaseManagementClient = result.AsT0;

        var getDatabaseNamesResult = await databaseManagementClient.GetDatabaseNames(cancellationToken);
        return getDatabaseNamesResult
            .Match<OneOf<IEnumerable<DatabaseInfoModel>, IEnumerable<Err>>>(f0 => f0, f1 => f1);


        //ასეთი კონსტრუქცია ვერ გავმართე
        //return await Task.FromResult(result.Match(x => x.GetDatabaseNames(request.ServerName).Result, er => er.ToArray()));
    }
}