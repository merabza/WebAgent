using MessagingAbstractions;

namespace LibDatabasesMini.CommandRequests;

public sealed class RecompileProceduresCommandRequest : ICommand
{
    public RecompileProceduresCommandRequest(string databaseName)
    {
        DatabaseName = databaseName;
    }

    public string DatabaseName { get; set; }

    public static RecompileProceduresCommandRequest Create(string databaseName)
    {
        return new RecompileProceduresCommandRequest(databaseName);
    }
}