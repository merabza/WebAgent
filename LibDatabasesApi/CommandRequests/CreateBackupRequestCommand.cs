using ParametersManagement.LibDatabaseParameters;
using SystemTools.MediatRMessagingAbstractions;
using WebAgentContracts.WebAgentDatabasesApiContracts.V1.Responses;

namespace LibDatabasesApi.CommandRequests;

public sealed class CreateBackupRequestCommand : ICommand<BackupFileParameters>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public CreateBackupRequestCommand(string databaseName, string dbServerFoldersSetName,
        DatabaseBackupParametersDomain? dbBackupParameters, string? userName)
    {
        DatabaseName = databaseName;
        DbServerFoldersSetName = dbServerFoldersSetName;
        DbBackupParameters = dbBackupParameters;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string DbServerFoldersSetName { get; set; }
    public DatabaseBackupParametersDomain? DbBackupParameters { get; }
    public string? UserName { get; set; }

    //public static CreateBackupCommandRequest Create(string databaseName, string dbServerFoldersSetName,
    //    DatabaseBackupParametersModel? dbBackupParameters, string? userName)
    //{
    //    return new CreateBackupCommandRequest(databaseName, dbServerFoldersSetName, userName);
    //}
}
