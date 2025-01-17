using DbTools.Models;
using MessagingAbstractions;
using System.Collections.Generic;

namespace LibDatabasesApi.CommandRequests;

public sealed class GetDatabaseNamesCommandRequest : ICommand<IEnumerable<DatabaseInfoModel>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public GetDatabaseNamesCommandRequest(string? userName)
    {
        UserName = userName;
    }

    public string? UserName { get; set; }

    public static GetDatabaseNamesCommandRequest Create(string? userName)
    {
        return new GetDatabaseNamesCommandRequest(userName);
    }
}