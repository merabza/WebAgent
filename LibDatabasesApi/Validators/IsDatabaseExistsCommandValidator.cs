using FluentValidation;
using LibDatabasesApi.CommandRequests;
using LibProjectsApi.Validators;

namespace LibDatabasesApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class IsDatabaseExistsCommandValidator : AbstractValidator<IsDatabaseExistsRequestCommand>
{
    public IsDatabaseExistsCommandValidator()
    {
        //RuleFor(x => x.ServerName).FileName();
        RuleFor(x => x.DatabaseName).FileName();
    }
}