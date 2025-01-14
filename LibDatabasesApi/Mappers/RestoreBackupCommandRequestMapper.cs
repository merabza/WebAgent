using LibDatabasesApi.CommandRequests;
using WebAgentDatabasesApiContracts.V1.Requests;

namespace LibDatabasesApi.Mappers;

public static class RestoreBackupCommandRequestMapper
{
    public static RestoreBackupCommandRequest AdaptTo(this RestoreBackupRequest restoreBackupRequest,
        string databaseName, string dbServerFoldersSetName, string? userName)
    {
        return new RestoreBackupCommandRequest(databaseName, dbServerFoldersSetName, restoreBackupRequest.Prefix,
            restoreBackupRequest.Suffix, restoreBackupRequest.Name, restoreBackupRequest.DateMask, userName);
    }
}