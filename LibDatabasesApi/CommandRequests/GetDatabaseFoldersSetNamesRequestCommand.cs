using System.Collections.Generic;
using MediatRMessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class GetDatabaseFoldersSetNamesRequestCommand : ICommand<IEnumerable<string>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public GetDatabaseFoldersSetNamesRequestCommand(string? userName)
    {
        UserName = userName;
    }

    public string? UserName { get; set; }

    public static GetDatabaseFoldersSetNamesRequestCommand Create(string? userName)
    {
        return new GetDatabaseFoldersSetNamesRequestCommand(userName);
    }
}