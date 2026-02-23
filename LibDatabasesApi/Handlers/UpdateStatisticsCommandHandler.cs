using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LibDatabasesApi.CommandRequests;
using LibDatabasesApi.Helpers;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;
using ToolsManagement.DatabasesManagement;
using WebAgentShared.LibWebAgentData.ErrorModels;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class UpdateStatisticsCommandHandler : ICommandHandler<UpdateStatisticsRequestCommand>
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UpdateStatisticsCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public UpdateStatisticsCommandHandler(IConfiguration config, ILogger<UpdateStatisticsCommandHandler> logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<Unit, Err[]>> Handle(UpdateStatisticsRequestCommand request,
        CancellationToken cancellationToken)
    {
        OneOf<IDatabaseManager, Err[]> result = await DatabaseManagerCreator.Create(_config, _logger,
            _httpClientFactory, _messagesDataManager, request.UserName, cancellationToken);
        if (result.IsT1)
        {
            return result.AsT1.ToArray();
        }

        IDatabaseManager? databaseManagementClient = result.AsT0;

        if (await databaseManagementClient.UpdateStatistics(request.DatabaseName, cancellationToken))
        {
            return new Unit();
        }

        Err err = DbApiErrors.CannotCheckAndRepairDatabase(request.DatabaseName);
        _logger.LogError("{ErrorMessage}", err.ErrorMessage);
        return new[] { err };
    }
}
