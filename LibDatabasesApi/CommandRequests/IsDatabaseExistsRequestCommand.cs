using SystemTools.MediatRMessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class IsDatabaseExistsRequestCommand : ICommand<bool>
{
    public IsDatabaseExistsRequestCommand(string databaseName, string? userName)
    {
        DatabaseName = databaseName;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string? UserName { get; set; }

    public static IsDatabaseExistsRequestCommand Create(string databaseName, string? userName)
    {
        return new IsDatabaseExistsRequestCommand(databaseName, userName);
    }
}
