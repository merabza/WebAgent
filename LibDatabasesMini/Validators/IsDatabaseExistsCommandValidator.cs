using FluentValidation;
using LibDatabasesMini.CommandRequests;
using LibProjectsMini.Validators;

namespace LibDatabasesMini.Validators;

// ReSharper disable once UnusedType.Global
public sealed class IsDatabaseExistsCommandValidator : AbstractValidator<IsDatabaseExistsCommandRequest>
{
    public IsDatabaseExistsCommandValidator()
    {
        //RuleFor(x => x.ServerName).FileName();
        RuleFor(x => x.DatabaseName).FileName();
    }
}