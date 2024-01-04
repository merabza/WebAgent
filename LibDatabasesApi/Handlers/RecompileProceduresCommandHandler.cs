﻿using System.Collections.Generic;
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

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class RecompileProceduresCommandHandler : ICommandHandler<RecompileProceduresCommandRequest>
{
    private readonly IConfiguration _config;
    private readonly ILogger<RecompileProceduresCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    public RecompileProceduresCommandHandler(IConfiguration config, ILogger<RecompileProceduresCommandHandler> logger,
        IMessagesDataManager messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(RecompileProceduresCommandRequest request,
        CancellationToken cancellationToken)
    {
        var result = await DatabaseClientCreator.Create(_config, _logger, _messagesDataManager, request.UserName,
            cancellationToken);
        if (result.IsT1)
            return result.AsT1.ToArray();
        var databaseManagementClient = result.AsT0;

        if (await databaseManagementClient.RecompileProcedures(request.DatabaseName, cancellationToken))
            return new Unit();

        var err = DbApiErrors.CannotRecompileProcedures(request.DatabaseName);
        _logger.LogError(err.ErrorMessage);
        return new[] { err };
    }
}