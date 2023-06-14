using System;
using System.IO;
using FluentValidationInstaller;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SystemToolsShared;
using WebInstallers;
using AssemblyReference = TestToolsMini.AssemblyReference;

//პროგრამის ატრიბუტების დაყენება 
ProgramAttributes.Instance.SetAttribute("AppName", "WebAgent");
ProgramAttributes.Instance.SetAttribute("VersionCount", 1);
ProgramAttributes.Instance.SetAttribute("UseSwaggerWithJWTBearer", false);
ProgramAttributes.Instance.SetAttribute("AppKey", "E0FFB24C-7561-4DBA-8E0F-02CA585A3C9C");

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory,
    Args = args
});

builder.InstallServices(args,
    AssemblyReference.Assembly,
    LibProjectsMini.AssemblyReference.Assembly,
    LibDatabasesMini.AssemblyReference.Assembly,
    ConfigurationEncrypt.AssemblyReference.Assembly,
    SerilogLogger.AssemblyReference.Assembly,
    WindowsServiceTools.AssemblyReference.Assembly,
    SwaggerTools.AssemblyReference.Assembly,
    ApiExceptionHandler.AssemblyReference.Assembly
);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(LibProjectsMini.AssemblyReference.Assembly);
    cfg.RegisterServicesFromAssembly(LibDatabasesMini.AssemblyReference.Assembly);
});

// FluentValidationInstaller
builder.Services.InstallValidation(LibProjectsMini.AssemblyReference.Assembly);

var app = builder.Build();

app.UseServices();

try
{
    Log.Information("Directory.GetCurrentDirectory() = {0}", Directory.GetCurrentDirectory());
    Log.Information("AppContext.BaseDirectory = {0}", AppContext.BaseDirectory);

    app.Run();

    Log.Information("Finish");
    return 0;
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}