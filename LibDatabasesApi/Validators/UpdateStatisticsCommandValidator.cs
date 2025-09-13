using FluentValidation;
using LibDatabasesApi.CommandRequests;
using LibProjectsApi.Validators;

namespace LibDatabasesApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class UpdateStatisticsCommandValidator : AbstractValidator<UpdateStatisticsRequestCommand>
{
    public UpdateStatisticsCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
    }
}