using MessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class ExecuteCommandCommandRequest : ICommand
{
    public ExecuteCommandCommandRequest(string databaseName, string? commandText, string? userName)
    {
        DatabaseName = databaseName;
        CommandText = commandText;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string? CommandText { get; set; }
    public string? UserName { get; set; }

    public static ExecuteCommandCommandRequest Create(string databaseName, string? commandText, string? userName)
    {
        return new ExecuteCommandCommandRequest(databaseName, commandText, userName);
    }
}