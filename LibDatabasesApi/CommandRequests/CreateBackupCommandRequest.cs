using DbTools;
using MessagingAbstractions;
using WebAgentProjectsApiContracts.V1.Responses;

namespace LibDatabasesApi.CommandRequests;

public sealed class CreateBackupCommandRequest : ICommand<BackupFileParameters>
{
    public CreateBackupCommandRequest(string databaseName, string? backupNamePrefix,
        string? dateMask, string? backupFileExtension, string? backupNameMiddlePart, bool compress, bool verify,
        EBackupType backupType, string? dbServerSideBackupPath, string? userName)
    {
        DatabaseName = databaseName;
        BackupNamePrefix = backupNamePrefix;
        DateMask = dateMask;
        BackupFileExtension = backupFileExtension;
        BackupNameMiddlePart = backupNameMiddlePart;
        Compress = compress;
        Verify = verify;
        BackupType = backupType;
        DbServerSideBackupPath = dbServerSideBackupPath;
        UserName = userName;
    }

    public string DatabaseName { get; set; }
    public string? BackupNamePrefix { get; set; }
    public string? DateMask { get; set; }
    public string? BackupFileExtension { get; set; }
    public string? BackupNameMiddlePart { get; set; }

    public bool Compress { get; set; }
    public bool Verify { get; set; }
    public EBackupType BackupType { get; set; }
    public string? DbServerSideBackupPath { get; set; }
    public string? UserName { get; set; }
}