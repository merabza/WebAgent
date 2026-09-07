using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibDatabasesApi.CommandRequests;
using Microsoft.Extensions.Configuration;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using WebAgentShared.LibProjectsApi;
using WebAgentShared.LibWebAgentData;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class
    GetDatabaseConnectionNamesCommandHandler : ICommandHandler<GetDatabaseConnectionNamesRequestCommand, string[]>
{
    private readonly IConfiguration _config;

    public GetDatabaseConnectionNamesCommandHandler(IConfiguration config)
    {
        _config = config;
    }

    public Task<Result<string[]>> Handle(GetDatabaseConnectionNamesRequestCommand request,
        CancellationToken cancellationToken)
    {
        var appSettings = AppSettings.Create(_config);

        if (appSettings is null)
        {
            return Task.FromResult(Result.Failure<string[]>(ProjectsErrors.AppSettingsIsNotCreated));
        }

        return Task.FromResult(Result.Success(appSettings.DatabaseServerConnections.Keys.ToArray()));
    }
}
