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
using WebAgentShared.LibWebAgentData.ErrorModels;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class TestConnectionCommandHandler : ICommandHandler<TestConnectionRequestCommand>
{
    private readonly IApplication _application;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TestConnectionCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public TestConnectionCommandHandler(IConfiguration config, ILogger<TestConnectionCommandHandler> logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager messagesDataManager, IApplication application)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
        _application = application;
    }

    public async Task<Result> Handle(TestConnectionRequestCommand request, CancellationToken cancellationToken)
    {
        Result<IDatabaseManager> databaseClientCreatorResult = await DatabaseManagerCreator.Create(
            _application.AppName, _config, _logger, _httpClientFactory, _messagesDataManager, request.UserName,
            cancellationToken);
        if (databaseClientCreatorResult.IsFailure)
        {
            return databaseClientCreatorResult.Error;
        }

        IDatabaseManager databaseManagementClient = databaseClientCreatorResult.Value;

        Result testResult = await databaseManagementClient.TestConnection(request.DatabaseName, cancellationToken);
        if (testResult.IsSuccess)
        {
            return Result.Success();
        }

        Error err = DbApiErrors.TestConnectionFailed(request.DatabaseName);
        _logger.LogError("{Description}", err.Description);
        return new ValidationError([.. testResult.Error.ToErrorArray(), err]);
    }
}
