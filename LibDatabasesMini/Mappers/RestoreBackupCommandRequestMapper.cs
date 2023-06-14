using LibDatabasesMini.CommandRequests;
using WebAgentContracts.V1.Requests;

namespace LibDatabasesMini.Mappers;

public static class RestoreBackupCommandRequestMapper
{
    public static RestoreBackupCommandRequest AdaptTo(this RestoreBackupRequest restoreBackupRequest,
        string databaseName)
    {
        return new RestoreBackupCommandRequest(databaseName, restoreBackupRequest.Prefix, restoreBackupRequest.Suffix,
            restoreBackupRequest.Name, restoreBackupRequest.DateMask);
    }
}