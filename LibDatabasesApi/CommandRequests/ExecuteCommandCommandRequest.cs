using MessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class ExecuteCommandCommandRequest : ICommand
{
    public ExecuteCommandCommandRequest(string databaseName, string? commandText)
    {
        DatabaseName = databaseName;
        CommandText = commandText;
    }

    public string DatabaseName { get; set; }
    public string? CommandText { get; set; }

    public static ExecuteCommandCommandRequest Create(string databaseName, string? commandText)
    {
        return new ExecuteCommandCommandRequest(databaseName, commandText);
    }
}