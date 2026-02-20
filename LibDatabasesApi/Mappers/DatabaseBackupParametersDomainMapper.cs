using ParametersManagement.LibDatabaseParameters;
using WebAgentContracts.WebAgentDatabasesApiContracts.V1.Requests;

namespace LibDatabasesApi.Mappers;

public static class DatabaseBackupParametersDomainMapper
{
    public static DatabaseBackupParametersDomain AdaptTo(this CreateDatabaseBackupRequest createBackupRequest)
    {
        return new DatabaseBackupParametersDomain(createBackupRequest.BackupNamePrefix, createBackupRequest.DateMask,
            createBackupRequest.BackupFileExtension, createBackupRequest.BackupNameMiddlePart,
            createBackupRequest.Compress, createBackupRequest.Verify, createBackupRequest.BackupType);
    }
}
