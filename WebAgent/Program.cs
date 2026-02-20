using System;
using System.IO;
using System.Reflection;
using ApiKeyIdentity.DependencyInjection;
using ConfigurationEncrypt;
using Figgle.Fonts;
using LibDatabasesApi.DependencyInjection;
using LibProjectsApi;
using LibProjectsApi.DependencyInjection;
using MediatorTools.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SerilogLogger;
using SignalRMessages.DependencyInjection;
using SwaggerTools.DependencyInjection;
using TestToolsApi.DependencyInjection;
using WindowsServiceTools;

//using AssemblyReference = ApiExceptionHandler.AssemblyReference;

try
{
    Console.WriteLine("Loading...");

    const string appName = "WebAgent";
    const string appKey = "E0FFB24C-7561-4DBA-8E0F-02CA585A3C9C";
    const int versionCount = 1;

    string header = $"{appName} {Assembly.GetEntryAssembly()?.GetName().Version}";
    Console.WriteLine(FiggleFonts.Standard.Render(header));

    //var parameters = new Dictionary<string, string>
    //{
    //    //{ SignalRMessagesInstaller.SignalRReCounterKey, string.Empty },//Allow SignalRReCounterKey
    //    { ConfigurationEncryptInstaller.AppKeyKey, appKey },
    //    { SwaggerInstaller.AppNameKey, appName },
    //    { SwaggerInstaller.VersionCountKey, 1.ToString() }
    //    //{ SwaggerInstaller.UseSwaggerWithJwtBearerKey, string.Empty },//Allow Swagger
    //};

    WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        ContentRootPath = AppContext.BaseDirectory, Args = args
    });

    bool debugMode = builder.Environment.IsDevelopment();

    ILogger logger = builder.Host.UseSerilogLogger(debugMode, builder.Configuration);
    ILogger? debugLogger = debugMode ? logger : null;

    builder.Host.UseWindowsServiceOnWindows(debugLogger, args);

    builder.Configuration.AddConfigurationEncryption(debugLogger, appKey);

    // @formatter:off
    builder.Services.AddHttpClient()
        .AddSwagger(debugLogger, true, versionCount, appName) //+
        .AddApiKeyIdentity(debugLogger)
        .AddSignalRMessages(debugLogger)
        //.AddSupportToolsServerRepositories(debugMode)
        //.AddSupportToolsServerPersistence(builder.Configuration, debugMode)
        .AddMediator(debugLogger,
            builder.Configuration, 
            AssemblyReference.Assembly, 
            LibDatabasesApi.AssemblyReference.Assembly);
        //.AddSupportToolsServerApiKeyIdentity(debugMode)
        //.AddAllScopedServiceSupportToolsServerApplication()
        //.AddSupportToolsServerQueryRepositories(debugMode)
        //.AddSupportToolsServerCommandRepositories(debugMode)
        //.AddSupportToolsServerForCommandsDatabase(builder.Configuration, debugMode)
        //.AddSupportToolsServer_Repositories(debugMode)
        //.AddSupportToolsServerDb(builder.Configuration, debugMode);
    // @formatter:on

    //if (!builder.InstallServices(debugMode, args, parameters,

    //        // @formatter:off

    //        //WebSystemTools
    //        AssemblyReference.Assembly, 
    //        ApiKeyIdentity.AssemblyReference.Assembly,
    //        ConfigurationEncrypt.AssemblyReference.Assembly, 
    //        FluentValidationInstaller.AssemblyReference.Assembly,
    //        HttpClientInstaller.AssemblyReference.Assembly, 
    //        SerilogLogger.AssemblyReference.Assembly,
    //        SignalRMessages.AssemblyReference.Assembly, 
    //        SwaggerTools.AssemblyReference.Assembly,
    //        TestToolsApi.AssemblyReference.Assembly, 
    //        WindowsServiceTools.AssemblyReference.Assembly,

    //        //WebAgentShared
    //        LibProjectsApi.AssemblyReference.Assembly,

    //        //WebAgent
    //        LibDatabasesApi.AssemblyReference.Assembly))
    //    return 2;

    //var mediatRSettings = builder.Configuration.GetSection("MediatRLicenseKey");

    //var mediatRLicenseKey = mediatRSettings.Get<string>();

    //builder.Services.AddMediatR(cfg =>
    //{
    //    cfg.LicenseKey = mediatRLicenseKey;
    //    cfg.RegisterServicesFromAssembly(LibProjectsApi.AssemblyReference.Assembly);
    //    cfg.RegisterServicesFromAssembly(LibDatabasesApi.AssemblyReference.Assembly);
    //});

    //// FluentValidationInstaller
    //builder.Services.InstallValidation(LibProjectsApi.AssemblyReference.Assembly);

    // ReSharper disable once using
    using WebApplication app = builder.Build();

    // ReSharper disable once RedundantArgumentDefaultValue
    app.UseSwaggerServices(debugLogger, versionCount);
    app.UseTestToolsApiEndpoints(debugLogger);
    //app.UseSignalRRecounterMessages(debugMode);

    app.UseLibProjectsApi(debugMode);
    app.UseLibDatabasesApi(debugMode);

    //if (!app.UseServices(debugMode))
    //    return 3;

    Log.Information("Directory.GetCurrentDirectory() = {0}", Directory.GetCurrentDirectory());
    Log.Information("AppContext.BaseDirectory = {0}", AppContext.BaseDirectory);

    await app.RunAsync();

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
    await Log.CloseAndFlushAsync();
}
