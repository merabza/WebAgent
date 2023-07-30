using FluentValidation;
using LibDatabasesApi.CommandRequests;
using LibProjectsApi.Validators;

namespace LibDatabasesApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class UpdateStatisticsCommandValidator : AbstractValidator<UpdateStatisticsCommandRequest>
{
    public UpdateStatisticsCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
    }
}