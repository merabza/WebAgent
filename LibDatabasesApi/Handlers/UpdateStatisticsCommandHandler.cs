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
public sealed class UpdateStatisticsCommandHandler : ICommandHandler<UpdateStatisticsRequestCommand>
{
    private readonly IApplication _application;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UpdateStatisticsCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public UpdateStatisticsCommandHandler(IConfiguration config, ILogger<UpdateStatisticsCommandHandler> logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager messagesDataManager, IApplication application)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
        _application = application;
    }

    public async Task<Result> Handle(UpdateStatisticsRequestCommand request, CancellationToken cancellationToken)
    {
        Result<IDatabaseManager> result = await DatabaseManagerCreator.Create(_application.AppName, _config, _logger,
            _httpClientFactory, _messagesDataManager, request.UserName, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error;
        }

        IDatabaseManager databaseManagementClient = result.Value;

        Result updateStatisticsResult =
            await databaseManagementClient.UpdateStatistics(request.DatabaseName, cancellationToken);
        if (updateStatisticsResult.IsSuccess)
        {
            return Result.Success();
        }

        Error err = DbApiErrors.CannotUpdateStatistics(request.DatabaseName);
        _logger.LogError("{Description}", err.Description);
        return new ValidationError([.. updateStatisticsResult.Error.ToErrorArray(), err]);
    }
}
