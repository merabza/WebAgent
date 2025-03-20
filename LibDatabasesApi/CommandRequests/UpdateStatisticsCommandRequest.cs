using MediatRMessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class UpdateStatisticsCommandRequest : ICommand
{
    public UpdateStatisticsCommandRequest(string databaseName, string? userName)
    {
        DatabaseName = databaseName;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string? UserName { get; set; }

    public static UpdateStatisticsCommandRequest Create(string databaseName, string? userName)
    {
        return new UpdateStatisticsCommandRequest(databaseName, userName);
    }
}