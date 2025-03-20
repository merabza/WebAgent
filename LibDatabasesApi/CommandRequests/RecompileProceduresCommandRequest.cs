using MediatRMessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class RecompileProceduresCommandRequest : ICommand
{
    public RecompileProceduresCommandRequest(string databaseName, string? userName)
    {
        DatabaseName = databaseName;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string? UserName { get; set; }

    public static RecompileProceduresCommandRequest Create(string databaseName, string? userName)
    {
        return new RecompileProceduresCommandRequest(databaseName, userName);
    }
}