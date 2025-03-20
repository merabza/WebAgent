using MediatRMessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class CheckRepairDatabaseCommandRequest : ICommand
{
    public CheckRepairDatabaseCommandRequest(string databaseName, string? userName)
    {
        DatabaseName = databaseName;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string? UserName { get; set; }

    public static CheckRepairDatabaseCommandRequest Create(string databaseName, string? userName)
    {
        return new CheckRepairDatabaseCommandRequest(databaseName, userName);
    }
}