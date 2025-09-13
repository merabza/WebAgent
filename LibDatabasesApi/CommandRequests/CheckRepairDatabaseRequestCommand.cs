using MediatRMessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class CheckRepairDatabaseRequestCommand : ICommand
{
    public CheckRepairDatabaseRequestCommand(string databaseName, string? userName)
    {
        DatabaseName = databaseName;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string? UserName { get; set; }

    public static CheckRepairDatabaseRequestCommand Create(string databaseName, string? userName)
    {
        return new CheckRepairDatabaseRequestCommand(databaseName, userName);
    }
}