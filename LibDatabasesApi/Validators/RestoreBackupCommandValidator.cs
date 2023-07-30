using FluentValidation;
using LibDatabasesApi.CommandRequests;
using LibProjectsApi.Validators;

namespace LibDatabasesApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class RestoreBackupCommandValidator : AbstractValidator<RestoreBackupCommandRequest>
{
    public RestoreBackupCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
        RuleFor(x => x.Prefix).FileName();
        RuleFor(x => x.Suffix).FileName();
        RuleFor(x => x.Name).FileName();
        RuleFor(x => x.DateMask).DateMask();
    }
}