using MessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class RestoreBackupCommandRequest : ICommand
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public RestoreBackupCommandRequest(string databaseName, string dbServerFoldersSetName, string? prefix,
        string? suffix, string? name, string? dateMask, string? userName)
    {
        DatabaseName = databaseName;
        DbServerFoldersSetName = dbServerFoldersSetName;
        Prefix = prefix;
        Suffix = suffix;
        Name = name;
        DateMask = dateMask;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string DbServerFoldersSetName { get; }
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public string? Name { get; set; }
    public string? DateMask { get; set; }
    public string? UserName { get; set; }
}