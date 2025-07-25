﻿using System.Text;
using Agile.Framework.Ai;
using Agile.Framework.Authentication;
using Agile.Framework.AzureAiSearch;
using Agile.Framework.AzureDocumentIntelligence;
using Agile.Framework.BlobStorage;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.Common.Interfaces;
using Agile.Framework.CosmosDb;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace Agile.Framework;

public static class DependencyInjection
{
    public static IServiceCollection AddFramework(this IServiceCollection services, IConfiguration configuration)
    {
        // Register the encoding provider for .msg conversion
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        return services
            .AddSeriLogLogging(configuration)
            .AddAppKernelBuilder()
            .AddCosmosDb()
            .AddBlobStorage()
            .AddAzureAiSearch()
            .AddAzureDocumentIntelligence()
            .AddAgileAuthentication();
    }

    public static async Task InitializeServicesAsync(this IApplicationBuilder app)
    {
        var initializers = app.ApplicationServices.GetServices<IAsyncInitializer>();
        foreach (var initializer in initializers)
            await initializer.InitializeAsync();
    }

    public static bool IsLocal(this IHostEnvironment app) => app.IsEnvironment("Local");

    private static IServiceCollection AddSeriLogLogging(this IServiceCollection services, IConfiguration configuration) =>
        services.AddSerilog(opt =>
        {
            opt.ReadFrom.Configuration(configuration);
            opt.WriteTo.ApplicationInsights(Configs.AppInsightsConnectionString, new TraceTelemetryConverter());
        });
}