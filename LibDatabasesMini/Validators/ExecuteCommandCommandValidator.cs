using FluentValidation;
using LibDatabasesMini.CommandRequests;
using LibProjectsMini.Validators;

namespace LibDatabasesMini.Validators;

// ReSharper disable once UnusedType.Global
public sealed class ExecuteCommandCommandValidator : AbstractValidator<ExecuteCommandCommandRequest>
{
    public ExecuteCommandCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
        RuleFor(x => x.CommandText).NotEmpty();
    }
}