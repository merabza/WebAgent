using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibDatabasesApi.CommandRequests;
using LibDatabasesApi.Helpers;
using MessagingAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class IsDatabaseExistsCommandHandler : ICommandHandler<IsDatabaseExistsCommandRequest, bool>
{
    private readonly IConfiguration _config;
    private readonly ILogger<IsDatabaseExistsCommandHandler> _logger;
    private readonly IMessagesDataManager? _messagesDataManager;

    public IsDatabaseExistsCommandHandler(IConfiguration config, ILogger<IsDatabaseExistsCommandHandler> logger,
        IMessagesDataManager? messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<bool, IEnumerable<Err>>> Handle(IsDatabaseExistsCommandRequest request,
        CancellationToken cancellationToken)
    {
        var result = DatabaseClientCreator.Create(_config, _logger, _messagesDataManager, request.UserName);
        if (result.IsT1)
            return result.AsT1.ToArray();
        var databaseManagementClient = result.AsT0;

        return await databaseManagementClient.IsDatabaseExists(request.DatabaseName);
    }
}