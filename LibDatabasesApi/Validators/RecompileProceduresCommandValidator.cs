using FluentValidation;
using LibDatabasesApi.CommandRequests;
using WebAgentShared.LibProjectsApi.Validators;

namespace LibDatabasesApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class RecompileProceduresCommandValidator : AbstractValidator<RecompileProceduresRequestCommand>
{
    public RecompileProceduresCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
    }
}
