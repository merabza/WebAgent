using SystemTools.MediatRMessagingAbstractions;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.CommandRequests;

public sealed class TestConnectionRequestCommand : ICommand
{
    public TestConnectionRequestCommand(string? databaseName, string? userName)
    {
        DatabaseName = databaseName;
        UserName = userName;
    }

    public string? DatabaseName { get; set; }
    public string? UserName { get; set; }

    public static TestConnectionRequestCommand Create(string? databaseName, string? userName)
    {
        return new TestConnectionRequestCommand(databaseName, userName);
    }
}
