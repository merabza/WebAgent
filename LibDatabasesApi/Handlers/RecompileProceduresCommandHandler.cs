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

// ReSharper disable once UnusedType.Global
public sealed class RecompileProceduresCommandHandler : ICommandHandler<RecompileProceduresCommandRequest>
{
    private readonly IConfiguration _config;
    private readonly ILogger<RecompileProceduresCommandHandler> _logger;

    public RecompileProceduresCommandHandler(IConfiguration config, ILogger<RecompileProceduresCommandHandler> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(RecompileProceduresCommandRequest request,
        CancellationToken cancellationToken)
    {
        var result = DatabaseClientCreator.Create(_config, _logger);
        if (result.IsT1)
            return result.AsT1.ToArray();
        var databaseManagementClient = result.AsT0;

        if (await databaseManagementClient.RecompileProcedures(request.DatabaseName))
            return new Unit();

        var err = DbApiErrors.CannotRecompileProcedures(request.DatabaseName);
        _logger.LogError(err.ErrorMessage);
        return new[] { err };
    }
}