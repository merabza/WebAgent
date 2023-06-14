using FluentValidation;
using LibDatabasesMini.CommandRequests;
using LibProjectsMini.Validators;

namespace LibDatabasesMini.Validators;

// ReSharper disable once UnusedType.Global
public sealed class CreateBackupCommandValidator : AbstractValidator<CreateBackupCommandRequest>
{
    public CreateBackupCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
        RuleFor(x => x.BackupNamePrefix).FileName();
        RuleFor(x => x.DateMask).DateMask();
        RuleFor(x => x.BackupFileExtension).FileExtension();
        RuleFor(x => x.BackupNameMiddlePart).FileName();
        RuleFor(x => x.DbServerSideBackupPath).FilePath();
    }
}