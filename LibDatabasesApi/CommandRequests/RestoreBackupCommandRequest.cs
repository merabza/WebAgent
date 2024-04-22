using MessagingAbstractions;


namespace LibDatabasesApi.CommandRequests;

public sealed class RestoreBackupCommandRequest : ICommand
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public RestoreBackupCommandRequest(string? destinationDbServerSideDataFolderPath,
        string? destinationDbServerSideLogFolderPath, string databaseName, string? prefix, string? suffix, string? name,
        string? dateMask, string? userName)
    {
        DestinationDbServerSideDataFolderPath = destinationDbServerSideDataFolderPath;
        DestinationDbServerSideLogFolderPath = destinationDbServerSideLogFolderPath;
        DatabaseName = databaseName;
        Prefix = prefix;
        Suffix = suffix;
        Name = name;
        DateMask = dateMask;
        UserName = userName;
    }

    public string? DestinationDbServerSideDataFolderPath { get; }
    public string? DestinationDbServerSideLogFolderPath { get; }
    public string DatabaseName { get; set; }
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public string? Name { get; set; }
    public string? DateMask { get; set; }
    public string? UserName { get; set; }
}