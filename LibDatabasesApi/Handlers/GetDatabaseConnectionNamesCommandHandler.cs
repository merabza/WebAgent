using LibDatabasesApi.CommandRequests;
using LibWebAgentData;
using MessagingAbstractions;
using Microsoft.Extensions.Configuration;
using OneOf;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SystemToolsShared.Errors;
using WebAgentDatabasesApiContracts.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GetDatabaseConnectionNamesCommandHandler : ICommandHandler<GetDatabaseConnectionNamesCommandRequest,
    IEnumerable<string>>
{
    private readonly IConfiguration _config;

    public GetDatabaseConnectionNamesCommandHandler(IConfiguration config)
    {
        _config = config;
    }

    public async Task<OneOf<IEnumerable<string>, IEnumerable<Err>>> Handle(
        GetDatabaseConnectionNamesCommandRequest request, CancellationToken cancellationToken = default)
    {
        var appSettings = AppSettings.Create(_config);

        if (appSettings is null)
            return await Task.FromResult(new[] { DatabaseApiClientErrors.AppSettingsIsNotCreated });

        return await Task.FromResult(appSettings.DatabaseServerConnections.Keys);
    }
}