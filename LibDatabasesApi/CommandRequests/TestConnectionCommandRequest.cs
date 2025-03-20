using MediatRMessagingAbstractions;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.CommandRequests;

public sealed class TestConnectionCommandRequest : ICommand
{
    public TestConnectionCommandRequest(string? databaseName, string? userName)
    {
        DatabaseName = databaseName;
        UserName = userName;
    }

    public string? DatabaseName { get; set; }
    public string? UserName { get; set; }

    public static TestConnectionCommandRequest Create(string? databaseName, string? userName)
    {
        return new TestConnectionCommandRequest(databaseName, userName);
    }
}