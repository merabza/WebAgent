using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LibDatabasesApi.CommandRequests;
using LibDatabasesApi.Helpers;
using MessagingAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;
using SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GetDatabaseFoldersSetNamesCommandHandler : ICommandHandler<GetDatabaseFoldersSetNamesCommandRequest,
    IEnumerable<string>>
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GetDatabaseFoldersSetNamesCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public GetDatabaseFoldersSetNamesCommandHandler(IConfiguration config,
        ILogger<GetDatabaseFoldersSetNamesCommandHandler> logger, IHttpClientFactory httpClientFactory,
        IMessagesDataManager messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<IEnumerable<string>, IEnumerable<Err>>> Handle(
        GetDatabaseFoldersSetNamesCommandRequest request, CancellationToken cancellationToken = default)
    {
        var result = await DatabaseManagerCreator.Create(_config, _logger, _httpClientFactory, _messagesDataManager,
            request.UserName, cancellationToken);
        if (result.IsT1)
            return result.AsT1.ToArray();
        var databaseManagementClient = result.AsT0;

        var getDatabaseFoldersSetNamesResult = await databaseManagementClient.GetDatabaseFoldersSets(cancellationToken);
        return getDatabaseFoldersSetNamesResult.Match<OneOf<IEnumerable<string>, IEnumerable<Err>>>(f0 => f0.Keys,
            f1 => (Err[])f1);
    }
}