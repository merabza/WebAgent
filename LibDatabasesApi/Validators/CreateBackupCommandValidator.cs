using FluentValidation;
using LibDatabasesApi.CommandRequests;
using LibProjectsApi.Validators;

namespace LibDatabasesApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class CreateBackupCommandValidator : AbstractValidator<CreateBackupCommandRequest>
{
    public CreateBackupCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
        RuleFor(x => x.DbServerFoldersSetName).FileName();
    }
}