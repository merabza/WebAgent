using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LibDatabasesApi.CommandRequests;
using LibDatabasesApi.Helpers;
using LibWebAgentData.ErrorModels;
using MediatR;
using MessagingAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;
using SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class TestConnectionCommandHandler : ICommandHandler<TestConnectionCommandRequest>
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TestConnectionCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public TestConnectionCommandHandler(IConfiguration config, ILogger<TestConnectionCommandHandler> logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(TestConnectionCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        var databaseClientCreatorResult = await DatabaseManagerCreator.Create(_config, _logger, _httpClientFactory,
            _messagesDataManager, request.UserName, cancellationToken);
        if (databaseClientCreatorResult.IsT1)
            return databaseClientCreatorResult.AsT1.ToArray();
        var databaseManagementClient = databaseClientCreatorResult.AsT0;

        var testResult = await databaseManagementClient.TestConnection(request.DatabaseName, cancellationToken);
        if (testResult.IsNone)
            return new Unit();

        var err = DbApiErrors.TestConnectionFailed(request.DatabaseName);
        _logger.LogError(err.ErrorMessage);
        return await Task.FromResult(Err.RecreateErrors((Err[])testResult, err));
    }
}