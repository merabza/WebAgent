using System;
using LibDatabasesApi.Endpoints.V1;
using Microsoft.AspNetCore.Routing;

namespace LibDatabasesApi.DependencyInjection;

public static class LibDatabasesApiDependencyInjection
{
    public static bool UseLibDatabasesApi(this IEndpointRouteBuilder endpoints, bool debugMode)
    {
        if (debugMode)
        {
            Console.WriteLine($"{nameof(UseLibDatabasesApi)} Started");
        }

        endpoints.UseDatabasesEndpoints(debugMode);

        if (debugMode)
        {
            Console.WriteLine($"{nameof(UseLibDatabasesApi)} Finished");
        }

        return true;
    }
}
