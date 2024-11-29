using System;
using System.Collections.Generic;
using Agile.Framework.Common.EnvironmentVariables.Models;
using Microsoft.Extensions.Configuration;

namespace Agile.Framework.Common.EnvironmentVariables;

public static class Configs
{
    public static void InitializeConfigs(IConfiguration configuration) => config = configuration;
    private static IConfiguration config;
    
    public static string BlobStorageConnectionString => config.GetConnectionString("BlobStorage") ?? throw new NullReferenceException("BlobStorageConnectionString is null");
    public static string AppInsightsConnectionString => config.GetConnectionString("APPLICATIONINSIGHTS_CONNECTION_STRING") ?? throw new NullReferenceException("APPLICATIONINSIGHTS_CONNECTION_STRING is null");
    public static string AzureAiServicesKey => config["AzureAiServicesKey"] ?? throw new NullReferenceException("AzureAiServicesKey is null");
    public static AzureAdConfig AzureAd => config.GetSection("AzureAd").Get<AzureAdConfig>()!;
    public static AzureOpenAiConfig AzureOpenAi => config.GetSection("AzureOpenAi").Get<AzureOpenAiConfig>()!;
    public static AzureSearchConfig AzureSearch => config.GetSection("AzureSearch").Get<AzureSearchConfig>()!;
    public static CosmosDbConfig CosmosDb => config.GetSection("CosmosDb").Get<CosmosDbConfig>()!;
    public static AuditConfig Audit => config.GetSection("Audit").Get<AuditConfig>()!;
    public static List<string> AdminEmailAddresses => config.GetSection("AdminEmailAddresses").Get<List<string>>()!;
    
    #region UTS
    public static string UtsRoleApiEndpoint => config["UtsRoleApiEndpoint"] ?? throw new NullReferenceException("UtsRoleApiEndpoint is null");
    public static string UtsXApiKey => config["UtsXApiKey"] ?? throw new NullReferenceException("UtsXApiKey is null");
    #endregion
}