using Agile.Framework.Authentication;
using Agile.Framework.Common.EnvironmentVariables;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Agile.Framework;

public static class DependencyInjection
{
    public static IServiceCollection AddFramework(this IServiceCollection services) =>
        services
            .AddSeriLogLogging()
            .AddAzureAdAuth();
    
    
    private static IServiceCollection AddSeriLogLogging(this IServiceCollection services) =>
        services.AddSerilog(opt =>
            {
                opt.WriteTo.Console();
                opt.WriteTo.ApplicationInsights(new TelemetryConfiguration {InstrumentationKey = Configs.AppInsightsInstrumentationKey}, TelemetryConverter.Traces);
            });
    }