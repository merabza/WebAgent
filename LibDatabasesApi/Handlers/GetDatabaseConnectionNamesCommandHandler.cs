using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibDatabasesApi.CommandRequests;
using LibProjectsApi;
using LibWebAgentData;
using MediatRMessagingAbstractions;
using Microsoft.Extensions.Configuration;
using OneOf;
using SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GetDatabaseConnectionNamesCommandHandler : ICommandHandler<GetDatabaseConnectionNamesRequestCommand,
    string[]>
{
    private readonly IConfiguration _config;

    public GetDatabaseConnectionNamesCommandHandler(IConfiguration config)
    {
        _config = config;
    }

    public async Task<OneOf<string[], Err[]>> Handle(GetDatabaseConnectionNamesRequestCommand request,
        CancellationToken cancellationToken = default)
    {
        var appSettings = AppSettings.Create(_config);

        if (appSettings is null)
            return await Task.FromResult(new[] { ProjectsErrors.AppSettingsIsNotCreated });

        return await Task.FromResult(appSettings.DatabaseServerConnections.Keys.ToArray());
    }
}