﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LibDatabasesApi.CommandRequests;
using LibDatabasesApi.Helpers;
using LibWebAgentData.ErrorModels;
using MediatR;
using MediatRMessagingAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;
using SystemToolsShared.Errors;

namespace LibDatabasesApi.Handlers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class CheckRepairDatabaseCommandHandler : ICommandHandler<CheckRepairDatabaseCommandRequest>
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CheckRepairDatabaseCommandHandler> _logger;
    private readonly IMessagesDataManager _messagesDataManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CheckRepairDatabaseCommandHandler(IConfiguration config, ILogger<CheckRepairDatabaseCommandHandler> logger,
        IHttpClientFactory httpClientFactory, IMessagesDataManager messagesDataManager)
    {
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _messagesDataManager = messagesDataManager;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(CheckRepairDatabaseCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await DatabaseManagerCreator.Create(_config, _logger, _httpClientFactory, _messagesDataManager,
            request.UserName, cancellationToken);
        if (result.IsT1)
            return result.AsT1.ToArray();
        var databaseManagementClient = result.AsT0;

        if (await databaseManagementClient.CheckRepairDatabase(request.DatabaseName, cancellationToken))
            return new Unit();

        var err = DbApiErrors.CannotCheckAndRepairDatabase(request.DatabaseName);
        _logger.LogError(err.ErrorMessage);
        return new[] { err };
    }
}