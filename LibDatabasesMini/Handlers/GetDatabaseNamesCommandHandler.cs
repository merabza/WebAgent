﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DbTools.Models;
using LibDatabasesMini.CommandRequests;
using LibDatabasesMini.Helpers;
using MessagingAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;

namespace LibDatabasesMini.Handlers;

// ReSharper disable once UnusedType.Global
public sealed class
    GetDatabaseNamesCommandHandler : ICommandHandler<GetDatabaseNamesCommandRequest, IEnumerable<DatabaseInfoModel>>
{
    private readonly IConfiguration _config;
    private readonly ILogger<GetDatabaseNamesCommandHandler> _logger;

    public GetDatabaseNamesCommandHandler(IConfiguration config, ILogger<GetDatabaseNamesCommandHandler> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<OneOf<IEnumerable<DatabaseInfoModel>, IEnumerable<Err>>> Handle(
        GetDatabaseNamesCommandRequest request, CancellationToken cancellationToken)
    {
        var result = DatabaseClientCreator.Create(_config, _logger);
        if (result.IsT1)
            return result.AsT1.ToArray();
        var databaseManagementClient = result.AsT0;

        return await databaseManagementClient.GetDatabaseNames();

        //ასეთი კონსტრუქცია ვერ გავმართე
        //return await Task.FromResult(result.Match(x => x.GetDatabaseNames(request.ServerName).Result, er => er.ToArray()));
    }
}