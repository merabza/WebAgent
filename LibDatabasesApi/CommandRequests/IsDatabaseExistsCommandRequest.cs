using MediatRMessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class IsDatabaseExistsCommandRequest : ICommand<bool>
{
    public IsDatabaseExistsCommandRequest(string databaseName, string? userName)
    {
        DatabaseName = databaseName;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string? UserName { get; set; }

    public static IsDatabaseExistsCommandRequest Create(string databaseName, string? userName)
    {
        return new IsDatabaseExistsCommandRequest(databaseName, userName);
    }
}