using System;
using System.Collections.Generic;
using System.IO;
using ConfigurationEncrypt;
using FluentValidationInstaller;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SwaggerTools;
using WebInstallers;

//using AssemblyReference = ApiExceptionHandler.AssemblyReference;

//პროგრამის ატრიბუტების დაყენება 
//ProgramAttributes.Instance.SetAttribute("AppName", "WebAgent");
//ProgramAttributes.Instance.SetAttribute("VersionCount", 1);
//ProgramAttributes.Instance.SetAttribute("UseSwaggerWithJWTBearer", false);
//ProgramAttributes.Instance.SetAttribute("AppKey", "E0FFB24C-7561-4DBA-8E0F-02CA585A3C9C");
try
{
    var parameters = new Dictionary<string, string>
    {
        //{ SignalRMessagesInstaller.SignalRReCounterKey, string.Empty },//Allow SignalRReCounterKey
        { ConfigurationEncryptInstaller.AppKeyKey, "E0FFB24C-7561-4DBA-8E0F-02CA585A3C9C" },
        { SwaggerInstaller.AppNameKey, "WebAgent" },
        { SwaggerInstaller.VersionCountKey, 1.ToString() }
        //{ SwaggerInstaller.UseSwaggerWithJwtBearerKey, string.Empty },//Allow Swagger
    };


    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        ContentRootPath = AppContext.BaseDirectory,
        Args = args
    });

    builder.InstallServices(args, parameters,
        //WebSystemTools
        FluentValidationInstaller.AssemblyReference.Assembly,
        ConfigurationEncrypt.AssemblyReference.Assembly,
        HttpClientInstaller.AssemblyReference.Assembly,
        SerilogLogger.AssemblyReference.Assembly,
        SwaggerTools.AssemblyReference.Assembly,
        TestToolsApi.AssemblyReference.Assembly,
        WindowsServiceTools.AssemblyReference.Assembly,
        SignalRMessages.AssemblyReference.Assembly,

        //WebAgentShared
        LibProjectsApi.AssemblyReference.Assembly,
        ApiExceptionHandler.AssemblyReference.Assembly,

        //WebAgent
        LibDatabasesApi.AssemblyReference.Assembly
    );

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(LibProjectsApi.AssemblyReference.Assembly);
        cfg.RegisterServicesFromAssembly(LibDatabasesApi.AssemblyReference.Assembly);
    });

    // FluentValidationInstaller
    builder.Services.InstallValidation(LibProjectsApi.AssemblyReference.Assembly);

    // ReSharper disable once using
    using var app = builder.Build();

    app.UseServices();

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