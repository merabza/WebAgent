using FluentValidation;
using LibDatabasesApi.CommandRequests;
using LibProjectsApi.Validators;

namespace LibDatabasesApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class CheckRepairDatabaseCommandValidator : AbstractValidator<CheckRepairDatabaseCommandRequest>
{
    public CheckRepairDatabaseCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
    }
}