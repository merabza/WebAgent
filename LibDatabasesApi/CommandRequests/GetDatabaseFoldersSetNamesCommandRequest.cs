using MessagingAbstractions;
using System.Collections.Generic;

namespace LibDatabasesApi.CommandRequests;

public sealed class GetDatabaseFoldersSetNamesCommandRequest : ICommand<IEnumerable<string>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public GetDatabaseFoldersSetNamesCommandRequest(string? userName)
    {
        UserName = userName;
    }

    public string? UserName { get; set; }

    public static GetDatabaseFoldersSetNamesCommandRequest Create(string? userName)
    {
        return new GetDatabaseFoldersSetNamesCommandRequest(userName);
    }
}