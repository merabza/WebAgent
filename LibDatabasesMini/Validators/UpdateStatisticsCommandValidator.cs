using FluentValidation;
using LibDatabasesMini.CommandRequests;
using LibProjectsMini.Validators;

namespace LibDatabasesMini.Validators;

// ReSharper disable once UnusedType.Global
public sealed class UpdateStatisticsCommandValidator : AbstractValidator<UpdateStatisticsCommandRequest>
{
    public UpdateStatisticsCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
    }
}