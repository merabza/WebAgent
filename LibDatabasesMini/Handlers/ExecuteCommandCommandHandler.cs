using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibDatabasesMini.CommandRequests;
using LibDatabasesMini.Helpers;
using LibWebAgentData.ErrorModels;
using MediatR;
using MessagingAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;

namespace LibDatabasesMini.Handlers;

// ReSharper disable once UnusedType.Global
public sealed class ExecuteCommandCommandHandler : ICommandHandler<ExecuteCommandCommandRequest>
{
    private readonly IConfiguration _config;
    private readonly ILogger<ExecuteCommandCommandHandler> _logger;

    public ExecuteCommandCommandHandler(IConfiguration config, ILogger<ExecuteCommandCommandHandler> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(ExecuteCommandCommandRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CommandText))
            return await Task.FromResult(new[] { DbApiErrors.CommandTextIsEmpty });

        var result = DatabaseClientCreator.Create(_config, _logger);
        if (result.IsT1)
            return result.AsT1.ToArray();
        var databaseManagementClient = result.AsT0;

        if (await databaseManagementClient.ExecuteCommand(request.CommandText, request.DatabaseName))
            return new Unit();

        var err = DbApiErrors.CouldNotExecuteCommand(request.DatabaseName);
        _logger.LogError(err.ErrorMessage);
        return await Task.FromResult(new[] { err });
    }
}