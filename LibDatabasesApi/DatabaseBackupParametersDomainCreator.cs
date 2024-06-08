using ApiContracts.Errors;
using LibDatabaseParameters;
using LibDatabasesApi.CommandRequests;
using OneOf;
using System.Collections.Generic;
using SystemToolsShared.Errors;
using WebAgentDatabasesApiContracts.V1.Requests;

namespace LibDatabasesApi;

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