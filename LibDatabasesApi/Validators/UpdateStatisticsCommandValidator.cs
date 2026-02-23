using FluentValidation;
using LibDatabasesApi.CommandRequests;
using WebAgentShared.LibProjectsApi.Validators;

namespace LibDatabasesApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class UpdateStatisticsCommandValidator : AbstractValidator<UpdateStatisticsRequestCommand>
{
    public UpdateStatisticsCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
    }
}
