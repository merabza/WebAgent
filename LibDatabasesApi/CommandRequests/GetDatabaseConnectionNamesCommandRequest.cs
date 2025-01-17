using System.Collections.Generic;
using MessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class GetDatabaseConnectionNamesCommandRequest : ICommand<IEnumerable<string>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public GetDatabaseConnectionNamesCommandRequest(string? userName)
    {
        UserName = userName;
    }

    public string? UserName { get; set; }

    public static GetDatabaseConnectionNamesCommandRequest Create(string? userName)
    {
        return new GetDatabaseConnectionNamesCommandRequest(userName);
    }
}