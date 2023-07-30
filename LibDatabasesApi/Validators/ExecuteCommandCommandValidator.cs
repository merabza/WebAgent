using FluentValidation;
using LibDatabasesApi.CommandRequests;
using LibProjectsApi.Validators;

namespace LibDatabasesApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class ExecuteCommandCommandValidator : AbstractValidator<ExecuteCommandCommandRequest>
{
    public ExecuteCommandCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
        RuleFor(x => x.CommandText).NotEmpty();
    }
}