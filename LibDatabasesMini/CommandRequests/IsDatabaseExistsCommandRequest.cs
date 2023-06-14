using MessagingAbstractions;

namespace LibDatabasesMini.CommandRequests;

public sealed class IsDatabaseExistsCommandRequest : ICommand<bool>
{
    public IsDatabaseExistsCommandRequest(string databaseName)
    {
        //ServerName = serverName;
        DatabaseName = databaseName;
    }

    //public string ServerName { get; set; }
    public string DatabaseName { get; set; }

    public static IsDatabaseExistsCommandRequest Create(string databaseName)
    {
        return new IsDatabaseExistsCommandRequest(databaseName);
    }
}