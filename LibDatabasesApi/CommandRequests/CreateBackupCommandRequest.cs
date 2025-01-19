using MessagingAbstractions;
using WebAgentDatabasesApiContracts.V1.Responses;

namespace LibDatabasesApi.CommandRequests;

public sealed class CreateBackupCommandRequest : ICommand<BackupFileParameters>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    private CreateBackupCommandRequest(string databaseName, string dbServerFoldersSetName, string? userName)
    {
        DatabaseName = databaseName;
        DbServerFoldersSetName = dbServerFoldersSetName;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string DbServerFoldersSetName { get; set; }
    public string? UserName { get; set; }

    public static CreateBackupCommandRequest Create(string databaseName, string dbServerFoldersSetName,
        string? userName)
    {
        return new CreateBackupCommandRequest(databaseName, dbServerFoldersSetName, userName);
    }
}