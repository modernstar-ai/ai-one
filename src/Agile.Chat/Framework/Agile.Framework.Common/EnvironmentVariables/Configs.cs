using Agile.Framework.Common.EnvironmentVariables.Models;
using Microsoft.Extensions.Configuration;

namespace Agile.Framework.Common.EnvironmentVariables;

public static class Configs
{
    public static void InitializeConfigs(IConfiguration configuration) => config = configuration;
    private static IConfiguration config;
    
    public static string BlobStorageConnectionString => config.GetConnectionString("BlobStorage") ?? throw new NullReferenceException("BlobStorageConnectionString is null");
    public static string AppInsightsInstrumentationKey => config["AppInsightsInstrumentationKey"] ?? throw new NullReferenceException("AppInsightsInstrumentationKey is null");
    public static string AzureAiServicesKey => config["AzureAiServicesKey"] ?? throw new NullReferenceException("AzureAiServicesKey is null");
    public static AzureAdConfig AzureAd => config.GetSection("AzureAd").Get<AzureAdConfig>()!;
    public static AzureOpenAiConfig AzureOpenAi => config.GetSection("AzureOpenAi").Get<AzureOpenAiConfig>()!;
    public static AzureSearchConfig AzureSearch => config.GetSection("AzureSearch").Get<AzureSearchConfig>()!;
    public static CosmosDbConfig CosmosDb => config.GetSection("CosmosDb").Get<CosmosDbConfig>()!;
    public static List<string> AdminEmailAddresses => config.GetSection("AdminEmailAddresses").Get<List<string>>()!;
}