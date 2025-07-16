using Agile.Framework.Common.EnvironmentVariables.Models;
using Microsoft.Extensions.Configuration;

namespace Agile.Framework.Common.EnvironmentVariables;

public static class Configs
{
    public static void InitializeConfigs(IConfiguration configuration) => config = configuration;
    private static IConfiguration config;

    public static BlobConfig BlobStorage => config.GetSection("BlobStorage").Get<BlobConfig>() ?? throw new NullReferenceException("BlobStorage is null");
    public static string AppInsightsConnectionString => config.GetConnectionString("APPLICATIONINSIGHTS_CONNECTION_STRING") ?? throw new NullReferenceException("APPLICATIONINSIGHTS_CONNECTION_STRING is null");
    public static AzureAdConfig AzureAd => config.GetSection("AzureAd").Get<AzureAdConfig>()! ?? throw new NullReferenceException("AzureAd is null");
    public static PermissionsType PermissionsType => config.GetSection("PermissionsType").Get<PermissionsType?>() ?? PermissionsType.Default;
    public static AzureAdPermissionsConfig AzureAdPermissionsSettings => config.GetSection("AzureAdPermissionsSettings").Get<AzureAdPermissionsConfig>() ?? throw new NullReferenceException("AzureAdPermissionsSettings is null");
    public static AzureDocumentIntelligenceConfig AzureDocumentIntelligence => config.GetSection("AzureDocumentIntelligence").Get<AzureDocumentIntelligenceConfig>()! ?? throw new NullReferenceException("AzureDocumentIntelligence is null");
    public static AzureServiceBusConfig AzureServiceBus => config.GetSection("AzureServiceBus").Get<AzureServiceBusConfig>()! ?? throw new NullReferenceException("AzureServiceBus is null");
    public static AzureOpenAiConfig AzureOpenAi => config.GetSection("AzureOpenAi").Get<AzureOpenAiConfig>()! ?? throw new NullReferenceException("AzureOpenAi is null");
    public static AIServicesConfig AIServices => config.GetSection("AIServices").Get<AIServicesConfig>()! ?? throw new NullReferenceException("AIServices is null");
    public static AzureSearchConfig AzureSearch => config.GetSection("AzureSearch").Get<AzureSearchConfig>()! ?? throw new NullReferenceException("AzureSearch is null");
    public static CosmosDbConfig CosmosDb => config.GetSection("CosmosDb").Get<CosmosDbConfig>()! ?? throw new NullReferenceException("CosmosDb is null");
    public static AuditConfig Audit => config.GetSection("Audit").Get<AuditConfig>()! ?? throw new NullReferenceException("Audit is null");
    public static AppSettingsConfig AppSettings => config.GetSection("AppSettings").Get<AppSettingsConfig>()! ?? new AppSettingsConfig();
}