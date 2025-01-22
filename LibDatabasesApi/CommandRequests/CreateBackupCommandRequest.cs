using LibDatabaseParameters;
using MessagingAbstractions;
using WebAgentDatabasesApiContracts.V1.Responses;

namespace LibDatabasesApi.CommandRequests;

public sealed class CreateBackupCommandRequest : ICommand<BackupFileParameters>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public CreateBackupCommandRequest(string databaseName, string dbServerFoldersSetName,
        DatabaseBackupParametersModel? dbBackupParameters, string? userName)
    {
        DatabaseName = databaseName;
        DbServerFoldersSetName = dbServerFoldersSetName;
        DbBackupParameters = dbBackupParameters;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string DbServerFoldersSetName { get; set; }
    public DatabaseBackupParametersModel? DbBackupParameters { get; }
    public string? UserName { get; set; }

    //public static CreateBackupCommandRequest Create(string databaseName, string dbServerFoldersSetName,
    //    DatabaseBackupParametersModel? dbBackupParameters, string? userName)
    //{
    //    return new CreateBackupCommandRequest(databaseName, dbServerFoldersSetName, userName);
    //}
}