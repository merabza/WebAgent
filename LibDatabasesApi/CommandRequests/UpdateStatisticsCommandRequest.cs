using MessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class UpdateStatisticsCommandRequest : ICommand
{
    public UpdateStatisticsCommandRequest(string databaseName)
    {
        DatabaseName = databaseName;
    }

    public string DatabaseName { get; set; }

    public static UpdateStatisticsCommandRequest Create(string databaseName)
    {
        return new UpdateStatisticsCommandRequest(databaseName);
    }
}