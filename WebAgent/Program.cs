using System;
using System.Collections.Generic;
using System.IO;
using ConfigurationEncrypt;
using FluentValidationInstaller;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SwaggerTools;
using SystemToolsShared;
using WebInstallers;
using AssemblyReference = ApiExceptionHandler.AssemblyReference;

const string appName = "WebAgent";
const string appKey = "E0FFB24C-7561-4DBA-8E0F-02CA585A3C9C";

//პროგრამის ატრიბუტების დაყენება 
ProgramAttributes.Instance.AppName = appName;
ProgramAttributes.Instance.AppName = appKey;

try
{
    var parameters = new Dictionary<string, string>
    {
        //{ SignalRMessagesInstaller.SignalRReCounterKey, string.Empty },//Allow SignalRReCounterKey
        { ConfigurationEncryptInstaller.AppKeyKey, appKey },
        { SwaggerInstaller.AppNameKey, appName },
        { SwaggerInstaller.VersionCountKey, 1.ToString() }
        //{ SwaggerInstaller.UseSwaggerWithJwtBearerKey, string.Empty },//Allow Swagger
    };


    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        ContentRootPath = AppContext.BaseDirectory, Args = args
    });

    var debugMode = builder.Environment.IsDevelopment();

    if (!builder.InstallServices(debugMode, args, parameters,
            //WebSystemTools
            AssemblyReference.Assembly, ApiKeyIdentity.AssemblyReference.Assembly,
            ConfigurationEncrypt.AssemblyReference.Assembly, FluentValidationInstaller.AssemblyReference.Assembly,
            HttpClientInstaller.AssemblyReference.Assembly, SerilogLogger.AssemblyReference.Assembly,
            SignalRMessages.AssemblyReference.Assembly, SwaggerTools.AssemblyReference.Assembly,
            TestToolsApi.AssemblyReference.Assembly, WindowsServiceTools.AssemblyReference.Assembly,

            //WebAgentShared
            LibProjectsApi.AssemblyReference.Assembly,

            //WebAgent
            LibDatabasesApi.AssemblyReference.Assembly))
        return 2;

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(LibProjectsApi.AssemblyReference.Assembly);
        cfg.RegisterServicesFromAssembly(LibDatabasesApi.AssemblyReference.Assembly);
    });

    // FluentValidationInstaller
    builder.Services.InstallValidation(LibProjectsApi.AssemblyReference.Assembly);

    // ReSharper disable once using
    using var app = builder.Build();

    if (!app.UseServices(debugMode))
        return 3;

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