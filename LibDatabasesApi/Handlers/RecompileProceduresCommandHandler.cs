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
public sealed class RecompileProceduresCommandHandler : ICommandHandler<RecompileProceduresRequestCommand>
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RecompileProceduresCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public RecompileProceduresCommandHandler(IConfiguration config, ILogger<RecompileProceduresCommandHandler> logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<Unit, Err[]>> Handle(RecompileProceduresRequestCommand request,
        CancellationToken cancellationToken)
    {
        var result = await DatabaseManagerCreator.Create(_config, _logger, _httpClientFactory, _messagesDataManager,
            request.UserName, cancellationToken);
        if (result.IsT1)
        {
            return result.AsT1.ToArray();
        }

        var databaseManagementClient = result.AsT0;

        if (await databaseManagementClient.RecompileProcedures(request.DatabaseName, cancellationToken))
        {
            return new Unit();
        }

        var err = DbApiErrors.CannotRecompileProcedures(request.DatabaseName);
        _logger.LogError("{ErrorMessage}", err.ErrorMessage);
        return new[] { err };
    }
}
