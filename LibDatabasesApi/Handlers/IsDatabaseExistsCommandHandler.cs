using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
public sealed class IsDatabaseExistsCommandHandler : ICommandHandler<IsDatabaseExistsRequestCommand, bool>
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IsDatabaseExistsCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public IsDatabaseExistsCommandHandler(IConfiguration config, ILogger<IsDatabaseExistsCommandHandler> logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<bool, Err[]>> Handle(IsDatabaseExistsRequestCommand request,
        CancellationToken cancellationToken)
    {
        var result = await DatabaseManagerCreator.Create(_config, _logger, _httpClientFactory, _messagesDataManager,
            request.UserName, cancellationToken);
        if (result.IsT1)
        {
            return result.AsT1.ToArray();
        }

        var databaseManagementClient = result.AsT0;

        var isDatabaseExistsResult =
            await databaseManagementClient.IsDatabaseExists(request.DatabaseName, cancellationToken);

        return isDatabaseExistsResult.Match<OneOf<bool, Err[]>>(f0 => f0, f1 => f1);
    }
}
