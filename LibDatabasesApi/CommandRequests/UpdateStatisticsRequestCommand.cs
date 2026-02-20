using SystemTools.MediatRMessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class UpdateStatisticsRequestCommand : ICommand
{
    public UpdateStatisticsRequestCommand(string databaseName, string? userName)
    {
        DatabaseName = databaseName;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string? UserName { get; set; }

    public static UpdateStatisticsRequestCommand Create(string databaseName, string? userName)
    {
        return new UpdateStatisticsRequestCommand(databaseName, userName);
    }
}
