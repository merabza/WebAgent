using System.Collections.Generic;
using ApiToolsShared;
using LibDatabaseParameters;
using LibDatabasesMini.CommandRequests;
using OneOf;
using SystemToolsShared;
using WebAgentDbContracts.V1.Requests;

namespace LibDatabasesMini;

public static class DatabaseBackupParametersDomainCreator
{
    public static OneOf<DatabaseBackupParametersDomain, IEnumerable<Err>> Create(
        CreateBackupRequest? createBackupRequest)
    {
        if (createBackupRequest is null || string.IsNullOrWhiteSpace(createBackupRequest.BackupNamePrefix) ||
            string.IsNullOrWhiteSpace(createBackupRequest.BackupFileExtension) ||
            string.IsNullOrWhiteSpace(createBackupRequest.BackupNameMiddlePart))
            return new[] { ApiErrors.SomeRequestParametersAreNotValid };


        return new DatabaseBackupParametersDomain(createBackupRequest.BackupNamePrefix,
            string.IsNullOrWhiteSpace(createBackupRequest.DateMask) ? "yyyyMMddHHmmss" : createBackupRequest.DateMask,
            createBackupRequest.BackupFileExtension, createBackupRequest.BackupNameMiddlePart,
            createBackupRequest.Compress, createBackupRequest.Verify, createBackupRequest.BackupType,
            createBackupRequest.DbServerSideBackupPath);
    }

    public static OneOf<DatabaseBackupParametersDomain, IEnumerable<Err>> Create(
        CreateBackupCommandRequest? createBackupRequest)
    {
        if (createBackupRequest is null || string.IsNullOrWhiteSpace(createBackupRequest.BackupNamePrefix) ||
            string.IsNullOrWhiteSpace(createBackupRequest.BackupFileExtension) ||
            string.IsNullOrWhiteSpace(createBackupRequest.BackupNameMiddlePart))
            return new[] { ApiErrors.SomeRequestParametersAreNotValid };


        return new DatabaseBackupParametersDomain(createBackupRequest.BackupNamePrefix,
            string.IsNullOrWhiteSpace(createBackupRequest.DateMask) ? "yyyyMMddHHmmss" : createBackupRequest.DateMask,
            createBackupRequest.BackupFileExtension, createBackupRequest.BackupNameMiddlePart,
            createBackupRequest.Compress, createBackupRequest.Verify, createBackupRequest.BackupType,
            createBackupRequest.DbServerSideBackupPath);
    }
}