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
public sealed class RecompileProceduresCommandHandler : ICommandHandler<RecompileProceduresRequestCommand>
{
    private readonly IApplication _application;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RecompileProceduresCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public RecompileProceduresCommandHandler(IConfiguration config, ILogger<RecompileProceduresCommandHandler> logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager messagesDataManager, IApplication application)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
        _application = application;
    }

    public async Task<Result> Handle(RecompileProceduresRequestCommand request, CancellationToken cancellationToken)
    {
        Result<IDatabaseManager> result = await DatabaseManagerCreator.Create(_application.AppName, _config, _logger,
            _httpClientFactory, _messagesDataManager, request.UserName, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error;
        }

        IDatabaseManager databaseManagementClient = result.Value;

        Result recompileProceduresResult =
            await databaseManagementClient.RecompileProcedures(request.DatabaseName, cancellationToken);
        if (recompileProceduresResult.IsSuccess)
        {
            return Result.Success();
        }

        Error err = DbApiErrors.CannotRecompileProcedures(request.DatabaseName);
        _logger.LogError("{Description}", err.Description);
        return new ValidationError([.. recompileProceduresResult.Error.ToErrorArray(), err]);
    }
}
