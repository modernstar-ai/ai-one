using Agile.Framework.Authentication;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.Common.Interfaces;
using Agile.Framework.CosmosDb;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Agile.Framework;

public static class DependencyInjection
{
    public static IServiceCollection AddFramework(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddSeriLogLogging(configuration)
            .AddCosmosDb()
            .AddAzureAdAuth();
    
    public static async Task InitializeServicesAsync(this IApplicationBuilder app)
    {
        var initializers = app.ApplicationServices.GetServices<IAsyncInitializer>();
        foreach (var initializer in initializers)
            await initializer.InitializeAsync();
    }
    
    
    private static IServiceCollection AddSeriLogLogging(this IServiceCollection services, IConfiguration configuration) =>
        services.AddSerilog(opt => opt.ReadFrom.Configuration(configuration));
    }