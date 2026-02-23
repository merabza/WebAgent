using FluentValidation;
using LibDatabasesApi.CommandRequests;
using WebAgentShared.LibProjectsApi.Validators;

namespace LibDatabasesApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class CreateBackupCommandValidator : AbstractValidator<CreateBackupRequestCommand>
{
    public CreateBackupCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
        RuleFor(x => x.DbServerFoldersSetName).FileName();
    }
}
