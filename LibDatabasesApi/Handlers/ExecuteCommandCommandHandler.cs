using LibDatabasesApi.CommandRequests;
using LibDatabasesApi.Helpers;
using LibWebAgentData.ErrorModels;
using MediatR;
using MessagingAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SystemToolsShared;
using SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ExecuteCommandCommandHandler : ICommandHandler<ExecuteCommandCommandRequest>
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExecuteCommandCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public ExecuteCommandCommandHandler(IConfiguration config, ILogger<ExecuteCommandCommandHandler> logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(ExecuteCommandCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CommandText))
            return await Task.FromResult(new[] { DbApiErrors.CommandTextIsEmpty });

        var result = await DatabaseManagerCreator.Create(_config, _logger, _httpClientFactory, _messagesDataManager,
            request.UserName, cancellationToken);
        if (result.IsT1)
            return result.AsT1.ToArray();
        var databaseManagementClient = result.AsT0;

        if (await databaseManagementClient.ExecuteCommand(request.CommandText, request.DatabaseName, cancellationToken))
            return new Unit();

        var err = DbApiErrors.CouldNotExecuteCommand(request.DatabaseName);
        _logger.LogError(err.ErrorMessage);
        return await Task.FromResult(new[] { err });
    }
}