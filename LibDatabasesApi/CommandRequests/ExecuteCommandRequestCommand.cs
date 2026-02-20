using SystemTools.MediatRMessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class ExecuteCommandRequestCommand : ICommand
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ExecuteCommandRequestCommand(string databaseName, string? commandText, string? userName)
    {
        DatabaseName = databaseName;
        CommandText = commandText;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string? CommandText { get; set; }
    public string? UserName { get; set; }

    public static ExecuteCommandRequestCommand Create(string databaseName, string? commandText, string? userName)
    {
        return new ExecuteCommandRequestCommand(databaseName, commandText, userName);
    }
}
