using System.Collections.Generic;
using System.Linq;
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

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class TestConnectionCommandHandler : ICommandHandler<TestConnectionCommandRequest>
{
    private readonly IConfiguration _config;
    private readonly ILogger<TestConnectionCommandHandler> _logger;
    private readonly IMessagesDataManager? _messagesDataManager;

    public TestConnectionCommandHandler(IConfiguration config, ILogger<TestConnectionCommandHandler> logger,
        IMessagesDataManager? messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(TestConnectionCommandRequest request,
        CancellationToken cancellationToken)
    {
        var result = DatabaseClientCreator.Create(_config, _logger, _messagesDataManager, request.UserName);
        if (result.IsT1)
            return result.AsT1.ToArray();
        var databaseManagementClient = result.AsT0;

        if (await databaseManagementClient.TestConnection(request.DatabaseName))
            return new Unit();

        var err = DbApiErrors.TestConnectionFailed(request.DatabaseName);
        _logger.LogError(err.ErrorMessage);
        return new[] { err };
    }
}