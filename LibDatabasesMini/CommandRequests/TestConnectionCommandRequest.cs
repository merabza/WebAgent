using MessagingAbstractions;

namespace LibDatabasesMini.CommandRequests;

public sealed class TestConnectionCommandRequest : ICommand
{
    public TestConnectionCommandRequest(string? databaseName)
    {
        DatabaseName = databaseName;
    }

    public string? DatabaseName { get; set; }

    public static TestConnectionCommandRequest Create(string? databaseName)
    {
        return new TestConnectionCommandRequest(databaseName);
    }
}