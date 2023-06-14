using FluentValidation;
using LibDatabasesMini.CommandRequests;
using LibProjectsMini.Validators;

namespace LibDatabasesMini.Validators;

// ReSharper disable once UnusedType.Global
public sealed class TestConnectionCommandValidator : AbstractValidator<TestConnectionCommandRequest>
{
    public TestConnectionCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
    }
}