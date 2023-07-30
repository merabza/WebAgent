using MessagingAbstractions;

namespace LibDatabasesApi.CommandRequests;

public sealed class RestoreBackupCommandRequest : ICommand
{
    public RestoreBackupCommandRequest(string databaseName, string? prefix, string? suffix, string? name,
        string? dateMask)
    {
        DatabaseName = databaseName;
        Prefix = prefix;
        Suffix = suffix;
        Name = name;
        DateMask = dateMask;
    }

    public string DatabaseName { get; set; }
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public string? Name { get; set; }
    public string? DateMask { get; set; }
}