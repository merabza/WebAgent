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
public sealed class UpdateStatisticsCommandHandler : ICommandHandler<UpdateStatisticsCommandRequest>
{
    private readonly IConfiguration _config;
    private readonly ILogger<UpdateStatisticsCommandHandler> _logger;

    public UpdateStatisticsCommandHandler(IConfiguration config, ILogger<UpdateStatisticsCommandHandler> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(UpdateStatisticsCommandRequest request,
        CancellationToken cancellationToken)
    {
        var result = DatabaseClientCreator.Create(_config, _logger);
        if (result.IsT1)
            return result.AsT1.ToArray();
        var databaseManagementClient = result.AsT0;

        if (await databaseManagementClient.UpdateStatistics(request.DatabaseName))
            return new Unit();

        var err = DbApiErrors.CannotCheckAndRepairDatabase(request.DatabaseName);
        _logger.LogError(err.ErrorMessage);
        return new[] { err };
    }
}