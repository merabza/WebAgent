//using LibDatabasesMini.CommandRequests;
//using LibWebAgentData;
//using MediatR;
//using Microsoft.Extensions.Configuration;
//using OneOf;
//using SystemToolsShared;

//namespace LibDatabasesMini.Handlers;

//// ReSharper disable once UnusedType.Global
//public sealed class
//    GetDatabaseServerNamesCommandHandler : ICommandHandler<GetDatabaseServerNamesCommandRequest, IEnumerable<string>>
//{
//    private readonly IConfiguration _config;
//    //private readonly ILogger<GetDatabaseNamesCommandHandler> _logger;

//    public GetDatabaseServerNamesCommandHandler(IConfiguration config)
//    {
//        _config = config;
//    }

//    public async Task<OneOf<IEnumerable<string>, IEnumerable<Err>>> Handle(GetDatabaseServerNamesCommandRequest request,
//        CancellationToken cancellationToken)
//    {
//        var appSettings = AppSettings.Create(_config);

//        return await Task.FromResult(appSettings.DatabaseServers.Keys.ToArray());
//    }
//}

