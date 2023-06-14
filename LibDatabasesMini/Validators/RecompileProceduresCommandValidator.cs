using FluentValidation;
using LibDatabasesMini.CommandRequests;
using LibProjectsMini.Validators;

namespace LibDatabasesMini.Validators;

// ReSharper disable once UnusedType.Global
public sealed class RecompileProceduresCommandValidator : AbstractValidator<RecompileProceduresCommandRequest>
{
    public RecompileProceduresCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
    }
}