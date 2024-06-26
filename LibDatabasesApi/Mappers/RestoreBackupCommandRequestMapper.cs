﻿using LibDatabasesApi.CommandRequests;
using WebAgentDatabasesApiContracts.V1.Requests;

namespace LibDatabasesApi.Mappers;

public static class RestoreBackupCommandRequestMapper
{
    public static RestoreBackupCommandRequest AdaptTo(this RestoreBackupRequest restoreBackupRequest,
        string databaseName, string? userName)
    {
        return new RestoreBackupCommandRequest(
            restoreBackupRequest.DestinationDbServerSideDataFolderPath,
            restoreBackupRequest.DestinationDbServerSideLogFolderPath,
            databaseName, restoreBackupRequest.Prefix, restoreBackupRequest.Suffix,
            restoreBackupRequest.Name, restoreBackupRequest.DateMask, userName);
    }
}