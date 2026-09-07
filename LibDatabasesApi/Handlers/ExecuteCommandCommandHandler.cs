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
public sealed class ExecuteCommandCommandHandler : ICommandHandler<ExecuteCommandRequestCommand>
{
    private readonly IApplication _application;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExecuteCommandCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public ExecuteCommandCommandHandler(IConfiguration config, ILogger<ExecuteCommandCommandHandler> logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager messagesDataManager, IApplication application)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
        _application = application;
    }

    public async Task<Result> Handle(ExecuteCommandRequestCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CommandText))
        {
            return DbApiErrors.CommandTextIsEmpty;
        }

        Result<IDatabaseManager> result = await DatabaseManagerCreator.Create(_application.AppName, _config, _logger,
            _httpClientFactory, _messagesDataManager, request.UserName, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error;
        }

        IDatabaseManager databaseManagementClient = result.Value;

        Result executeCommandResult = await databaseManagementClient.ExecuteCommand(request.CommandText,
            request.DatabaseName, cancellationToken);
        if (executeCommandResult.IsSuccess)
        {
            return Result.Success();
        }

        Error err = DbApiErrors.CouldNotExecuteCommand(request.DatabaseName);
        _logger.LogError("{Description}", err.Description);
        return new ValidationError([.. executeCommandResult.Error.ToErrorArray(), err]);
    }
}
