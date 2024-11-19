using Agile.Framework.Authentication;
using Agile.Framework.Common.EnvironmentVariables;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Agile.Framework;

public static class DependencyInjection
{
    public static IServiceCollection AddFramework(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddSeriLogLogging(configuration)
            .AddAzureAdAuth();
    
    
    private static IServiceCollection AddSeriLogLogging(this IServiceCollection services, IConfiguration configuration) =>
        services.AddSerilog(opt => opt.ReadFrom.Configuration(configuration));
    }