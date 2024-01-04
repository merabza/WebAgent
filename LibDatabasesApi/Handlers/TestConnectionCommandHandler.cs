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

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class TestConnectionCommandHandler : ICommandHandler<TestConnectionCommandRequest>
{
    private readonly IConfiguration _config;
    private readonly ILogger<TestConnectionCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public TestConnectionCommandHandler(IConfiguration config, ILogger<TestConnectionCommandHandler> logger,
        IMessagesDataManager messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(TestConnectionCommandRequest request,
        CancellationToken cancellationToken)
    {
        var databaseClientCreatorResult = await DatabaseClientCreator.Create(_config, _logger, _messagesDataManager,
            request.UserName, cancellationToken);
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