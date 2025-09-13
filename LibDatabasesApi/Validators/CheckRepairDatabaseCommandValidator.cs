using FluentValidation;
using LibDatabasesApi.CommandRequests;
using LibProjectsApi.Validators;

namespace LibDatabasesApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class CheckRepairDatabaseCommandValidator : AbstractValidator<CheckRepairDatabaseRequestCommand>
{
    public CheckRepairDatabaseCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
    }
}