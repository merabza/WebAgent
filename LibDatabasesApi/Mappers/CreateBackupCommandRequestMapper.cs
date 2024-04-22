﻿using DatabasesManagement.Requests;
using LibDatabasesApi.CommandRequests;

namespace LibDatabasesApi.Mappers;

public static class CreateBackupCommandRequestMapper
{
    public static CreateBackupCommandRequest AdaptTo(this CreateBackupRequest createBackupRequest, string databaseName,
        string? userName)
    {
        return new CreateBackupCommandRequest(databaseName, createBackupRequest.BackupNamePrefix,
            createBackupRequest.DateMask, createBackupRequest.BackupFileExtension,
            createBackupRequest.BackupNameMiddlePart, createBackupRequest.Compress, createBackupRequest.Verify,
            createBackupRequest.BackupType, createBackupRequest.DbServerSideBackupPath, userName);
    }
}