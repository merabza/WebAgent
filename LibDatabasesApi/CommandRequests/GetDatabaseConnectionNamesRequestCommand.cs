using System.Collections.Generic;
using MediatRMessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class GetDatabaseConnectionNamesRequestCommand : ICommand<IEnumerable<string>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public GetDatabaseConnectionNamesRequestCommand(string? userName)
    {
        UserName = userName;
    }

    public string? UserName { get; set; }

    public static GetDatabaseConnectionNamesRequestCommand Create(string? userName)
    {
        return new GetDatabaseConnectionNamesRequestCommand(userName);
    }
}