using MessagingAbstractions;
// ReSharper disable ConvertToPrimaryConstructor

namespace LibDatabasesApi.CommandRequests;

public sealed class RestoreBackupCommandRequest : ICommand
{
    public RestoreBackupCommandRequest(string databaseName, string? prefix, string? suffix, string? name,
        string? dateMask, string? userName)
    {
        DatabaseName = databaseName;
        Prefix = prefix;
        Suffix = suffix;
        Name = name;
        DateMask = dateMask;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public string? Name { get; set; }
    public string? DateMask { get; set; }
    public string? UserName { get; set; }
}