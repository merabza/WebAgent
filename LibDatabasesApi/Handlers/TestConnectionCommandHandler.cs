using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LibDatabasesApi.CommandRequests;
using LibDatabasesApi.Helpers;
using LibWebAgentData.ErrorModels;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class TestConnectionCommandHandler : ICommandHandler<TestConnectionRequestCommand>
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

    public async Task<OneOf<Unit, Err[]>> Handle(TestConnectionRequestCommand request,
        CancellationToken cancellationToken)
    {
        var databaseClientCreatorResult = await DatabaseManagerCreator.Create(_config, _logger, _httpClientFactory,
            _messagesDataManager, request.UserName, cancellationToken);
        if (databaseClientCreatorResult.IsT1)
        {
            return databaseClientCreatorResult.AsT1.ToArray();
        }

        var databaseManagementClient = databaseClientCreatorResult.AsT0;

        var testResult = await databaseManagementClient.TestConnection(request.DatabaseName, cancellationToken);
        if (testResult.IsNone)
        {
            return new Unit();
        }

        var err = DbApiErrors.TestConnectionFailed(request.DatabaseName);
        _logger.LogError("{ErrorMessage}", err.ErrorMessage);
        return await Task.FromResult(Err.RecreateErrors((Err[])testResult, err));
    }
}
