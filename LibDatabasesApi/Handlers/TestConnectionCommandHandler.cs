using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using LibDatabasesApi.CommandRequests;
using LibDatabasesApi.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;
using ToolsManagement.DatabasesManagement;
using WebAgentShared.LibWebAgentData.ErrorModels;
using Unit = MediatR.Unit;

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

    public async Task<OneOf<Unit, Error[]>> Handle(TestConnectionRequestCommand request,
        CancellationToken cancellationToken)
    {
        OneOf<IDatabaseManager, Error[]> databaseClientCreatorResult = await DatabaseManagerCreator.Create(_config,
            _logger, _httpClientFactory, _messagesDataManager, request.UserName, cancellationToken);
        if (databaseClientCreatorResult.IsT1)
        {
            return databaseClientCreatorResult.AsT1.ToArray();
        }

        IDatabaseManager? databaseManagementClient = databaseClientCreatorResult.AsT0;

        Option<Error[]> testResult =
            await databaseManagementClient.TestConnection(request.DatabaseName, cancellationToken);
        if (testResult.IsNone)
        {
            return new Unit();
        }

        Error err = DbApiErrors.TestConnectionFailed(request.DatabaseName);
        _logger.LogError("{Name}", err.Name);
        return await Task.FromResult(Error.RecreateErrors((Error[])testResult, err));
    }
}
