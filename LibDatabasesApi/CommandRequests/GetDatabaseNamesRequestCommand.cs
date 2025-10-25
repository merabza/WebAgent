using DbTools.Models;
using MediatRMessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class GetDatabaseNamesRequestCommand : ICommand<DatabaseInfoModel[]>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public GetDatabaseNamesRequestCommand(string? userName)
    {
        UserName = userName;
    }

    public string? UserName { get; set; }

    public static GetDatabaseNamesRequestCommand Create(string? userName)
    {
        return new GetDatabaseNamesRequestCommand(userName);
    }
}