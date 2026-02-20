using SystemTools.MediatRMessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class RecompileProceduresRequestCommand : ICommand
{
    public RecompileProceduresRequestCommand(string databaseName, string? userName)
    {
        DatabaseName = databaseName;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string? UserName { get; set; }

    public static RecompileProceduresRequestCommand Create(string databaseName, string? userName)
    {
        return new RecompileProceduresRequestCommand(databaseName, userName);
    }
}
