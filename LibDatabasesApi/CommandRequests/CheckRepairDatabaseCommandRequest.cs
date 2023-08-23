using MessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class CheckRepairDatabaseCommandRequest : ICommand
{
    public CheckRepairDatabaseCommandRequest(string databaseName)
    {
        DatabaseName = databaseName;
    }

    public string DatabaseName { get; set; }

    public static CheckRepairDatabaseCommandRequest Create(string databaseName)
    {
        return new CheckRepairDatabaseCommandRequest(databaseName);
    }
}