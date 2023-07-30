using LibDatabasesApi.CommandRequests;
using LibDatabasesApi.Helpers;
using MessagingAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;

namespace LibDatabasesApi.Handlers;

// ReSharper disable once UnusedType.Global
public sealed class IsDatabaseExistsCommandHandler : ICommandHandler<IsDatabaseExistsCommandRequest, bool>
{
    private readonly IConfiguration _config;
    private readonly ILogger<IsDatabaseExistsCommandHandler> _logger;

    public IsDatabaseExistsCommandHandler(IConfiguration config, ILogger<IsDatabaseExistsCommandHandler> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<OneOf<bool, IEnumerable<Err>>> Handle(IsDatabaseExistsCommandRequest request,
        CancellationToken cancellationToken)
    {
        var result = DatabaseClientCreator.Create(_config, _logger);
        if (result.IsT1)
            return result.AsT1.ToArray();
        var databaseManagementClient = result.AsT0;

        return await databaseManagementClient.IsDatabaseExists(request.DatabaseName);
    }
}