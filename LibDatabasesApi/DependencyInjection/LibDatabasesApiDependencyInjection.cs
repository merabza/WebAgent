using System;
using LibDatabasesApi.Endpoints.V1;
using Microsoft.AspNetCore.Routing;
using Serilog;

namespace LibDatabasesApi.DependencyInjection;

public static class LibDatabasesApiDependencyInjection
{
    public static bool UseLibDatabasesApi(this IEndpointRouteBuilder endpoints, ILogger? debugLogger)
    {
        debugLogger?.Information("{MethodName} Started", nameof(UseLibDatabasesApi));

        endpoints.UseDatabasesEndpoints(debugLogger);

        debugLogger?.Information("{MethodName} Finished", nameof(UseLibDatabasesApi));

        return true;
    }
}
