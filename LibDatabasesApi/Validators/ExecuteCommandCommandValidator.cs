using FluentValidation;
using LibDatabasesApi.CommandRequests;
using WebAgentShared.LibProjectsApi.Validators;

namespace LibDatabasesApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class ExecuteCommandCommandValidator : AbstractValidator<ExecuteCommandRequestCommand>
{
    public ExecuteCommandCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
        RuleFor(x => x.CommandText).NotEmpty();
    }
}
