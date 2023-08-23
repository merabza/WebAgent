using LibDatabasesApi.CommandRequests;
using WebAgentProjectsApiContracts.V1.Requests;

namespace LibDatabasesApi.Mappers;

public static class RestoreBackupCommandRequestMapper
{
    public static RestoreBackupCommandRequest AdaptTo(this RestoreBackupRequest restoreBackupRequest,
        string databaseName)
    {
        return new RestoreBackupCommandRequest(databaseName, restoreBackupRequest.Prefix, restoreBackupRequest.Suffix,
            restoreBackupRequest.Name, restoreBackupRequest.DateMask);
    }
}