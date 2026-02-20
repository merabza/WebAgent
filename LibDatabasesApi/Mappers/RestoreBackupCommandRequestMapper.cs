using LibDatabasesApi.CommandRequests;
using WebAgentContracts.WebAgentDatabasesApiContracts.V1.Requests;

namespace LibDatabasesApi.Mappers;

public static class RestoreBackupCommandRequestMapper
{
    public static RestoreBackupCommandRequestCommand AdaptTo(this RestoreBackupRequest restoreBackupRequest,
        string databaseName, string dbServerFoldersSetName, string? userName)
    {
        return new RestoreBackupCommandRequestCommand(databaseName, dbServerFoldersSetName, restoreBackupRequest.Prefix,
            restoreBackupRequest.Suffix, restoreBackupRequest.Name, restoreBackupRequest.DateMask, userName,
            restoreBackupRequest.DatabaseRecoveryModel);
    }
}
