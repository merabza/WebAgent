using FluentValidation;
using LibDatabasesApi.CommandRequests;
using LibProjectsApi.Validators;

namespace LibDatabasesApi.Validators;

// ReSharper disable once UnusedType.Global
public sealed class TestConnectionCommandValidator : AbstractValidator<TestConnectionCommandRequest>
{
    public TestConnectionCommandValidator()
    {
        RuleFor(x => x.DatabaseName).FileName();
    }
}