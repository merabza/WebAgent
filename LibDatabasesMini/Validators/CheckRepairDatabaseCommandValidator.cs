using FluentValidation;
using LibDatabasesMini.CommandRequests;
using LibProjectsMini.Validators;

namespace LibDatabasesMini.Validators;

// ReSharper disable once UnusedType.Global
public sealed class CheckRepairDatabaseCommandValidator : AbstractValidator<CheckRepairDatabaseCommandRequest>
{
    public CheckRepairDatabaseCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
    }
}