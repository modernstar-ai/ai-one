namespace Agile.Framework.Common.EnvironmentVariables.Models;

public class AzureOpenAiConfig
{
    public AzureOpenAiApimConfig Apim { get; set; } = new();
    public string Endpoint { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiVersion { get; set; }
    public string? DeploymentName { get; set; }
    public string? EmbeddingsDeploymentName { get; set; }
    public string EmbeddingsModelName { get; set; }
}

public class AzureOpenAiApimConfig
{
    public string? Endpoint { get; set; }
    public string? EmbeddingsEndpoint { get; set; }
}
